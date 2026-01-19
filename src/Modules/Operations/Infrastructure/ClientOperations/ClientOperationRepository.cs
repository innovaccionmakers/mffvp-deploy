using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain.OperationTypes;
using Microsoft.EntityFrameworkCore;
using Operations.Domain.ClientOperations;
using Operations.Infrastructure.Database;

namespace Operations.Infrastructure.ClientOperations;

internal sealed class ClientOperationRepository(OperationsDbContext context) : IClientOperationRepository
{
    public async Task<IReadOnlyCollection<ClientOperation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.ClientOperations.ToListAsync(cancellationToken);
    }

    public async Task<ClientOperation?> GetAsync(long clientoperationId, CancellationToken cancellationToken = default)
    {
        return await context.ClientOperations
            .SingleOrDefaultAsync(x => x.ClientOperationId == clientoperationId, cancellationToken);
    }

    public void Insert(ClientOperation clientoperation)
    {
        context.ClientOperations.Add(clientoperation);
    }

    public void Update(ClientOperation clientoperation)
    {
        context.ClientOperations.Update(clientoperation);
    }

    public void Delete(ClientOperation clientoperation)
    {
        context.ClientOperations.Remove(clientoperation);
    }

    public async Task<bool> ExistsContributionAsync(
        int affiliateId,
        int objectiveId,
        int portfolioId,
        CancellationToken ct)
    {
        const string contributionLabel = "Aporte";

        return await context.ClientOperations
            .Where(op =>
                op.AffiliateId == affiliateId &&
                op.ObjectiveId == objectiveId &&
                op.PortfolioId == portfolioId &&
                op.Status == LifecycleStatus.Active)
            .Join(context.OperationTypes,
                op => op.OperationTypeId,
                st => st.OperationTypeId,
                (op, st) => st)
            .Where(st => st.CategoryId != null)
            .Join(context.OperationTypes,
                st => (long)st.CategoryId!.Value,
                ot => ot.OperationTypeId,
                (st, ot) => ot.Name)
            .AnyAsync(name => name == contributionLabel, ct);
    }

    public async Task<IEnumerable<ClientOperation>> GetClientOperationsByProcessDateAsync(DateTime processDate, CancellationToken cancellationToken = default)
    {
        var utcProcessDate = processDate.Kind == DateTimeKind.Unspecified
                            ? DateTime.SpecifyKind(processDate, DateTimeKind.Utc)
                            : processDate.ToUniversalTime();

        var clientOperations = await context.ClientOperations
            .Where(co => co.ProcessDate.Date == utcProcessDate.Date && co.Status == LifecycleStatus.Active)
            .Include(co => co.AuxiliaryInformation)
            .Include(co => co.OperationType)
            .ToListAsync(cancellationToken);

        var categoryIds = clientOperations
            .Where(op => op.OperationType?.CategoryId.HasValue == true)
            .Select(op => op.OperationType.CategoryId.Value)
            .Distinct()
            .ToList();

        var categories = await context.OperationTypes
            .Where(ot => categoryIds.Contains(ot.CategoryId.Value))
            .Select(ot => new { ot.OperationTypeId, ot.Name })
            .ToDictionaryAsync(ot => ot.OperationTypeId, ot => ot.Name, cancellationToken);

        foreach (var operation in clientOperations)
        {
            if (operation.OperationType?.CategoryId.HasValue == true &&
                categories.TryGetValue(operation.OperationType.CategoryId.Value, out var categoryName))
            {
                operation.OperationType.Name = categoryName;
            }
        }

        return clientOperations;
    }

    public async Task<IEnumerable<AccountingOperationsResponse>> GetAccountingOperationsAsync(IEnumerable<int> portfolioIds, DateTime processDate, CancellationToken cancellationToken = default)
    {
        if (portfolioIds == null || !portfolioIds.Any())
            return Enumerable.Empty<AccountingOperationsResponse>();

        return await context.ClientOperations
            .Where(co => co.Status == LifecycleStatus.Active)
            .Where(co => portfolioIds.Contains(co.PortfolioId) && co.ProcessDate == processDate)
            .Where(co => co.OperationType != null && co.OperationType.Name == OperationTypeAttributes.Names.None)
            .Include(co => co.AuxiliaryInformation)
            .Include(co => co.OperationType)
            .AsSplitQuery()
            .AsNoTracking()
            .Select(co => new AccountingOperationsResponse
            (
                co.ClientOperationId,
                co.PortfolioId,
                co.AffiliateId,
                co.Amount,
                co.OperationTypeId,
                co.AuxiliaryInformation.CollectionAccount
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AccountingOperationsResponse>> GetAccountingDebitNoteOperationsAsync(IEnumerable<int> portfolioIds, DateTime processDate, CancellationToken cancellationToken = default)
    {
        if (portfolioIds == null || !portfolioIds.Any())
            return Enumerable.Empty<AccountingOperationsResponse>();

        return await context.ClientOperations
            .Where(co => co.Status == LifecycleStatus.Active)
            .Where(co => portfolioIds.Contains(co.PortfolioId) && co.ProcessDate == processDate)
            .Where(co => co.OperationType != null && co.OperationType.Name == OperationTypeAttributes.Names.DebitNote)
            .Include(co => co.OperationType)
            .Include(co => co.LinkedClientOperation)
            .ThenInclude(co => co.AuxiliaryInformation)
            .Where(co => co.LinkedClientOperation != null && co.LinkedClientOperation.AuxiliaryInformation != null && !string.IsNullOrWhiteSpace(co.LinkedClientOperation.AuxiliaryInformation.CollectionAccount))
            .AsSplitQuery()
            .AsNoTracking()
            .Select(co => new AccountingOperationsResponse
            (
                co.ClientOperationId,
                co.PortfolioId,
                co.AffiliateId,
                co.Amount,
                co.OperationTypeId,
                co.LinkedClientOperation!.AuxiliaryInformation.CollectionAccount
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasActiveLinkedOperationAsync(
        long clientOperationId,
        long operationTypeId,
        CancellationToken cancellationToken = default)
    {
        return await context.ClientOperations
            .AnyAsync(
                co => co.LinkedClientOperationId == clientOperationId &&
                      co.OperationTypeId == operationTypeId &&
                      co.Status == LifecycleStatus.Active,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<ClientOperation>> GetContributionOperationsInRangeAsync(
        IReadOnlyCollection<long> contributionOperationTypeIds,
        int affiliateId,
        int objectiveId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        if (!TryPrepareContributionTypeIds(contributionOperationTypeIds, out var contributionTypeIdSet))
        {
            return Array.Empty<ClientOperation>();
        }

        if (affiliateId <= 0 || objectiveId <= 0)
        {
            return Array.Empty<ClientOperation>();
        }

        var (inclusiveStartDate, exclusiveEndDate) = NormalizeDateRange(startDate, endDate);

        return await BuildContributionOperationsQuery(contributionTypeIdSet, affiliateId, objectiveId)
            .Where(operation =>
                operation.ProcessDate >= inclusiveStartDate &&
                operation.ProcessDate < exclusiveEndDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ClientOperation>> GetContributionOperationsAsync(
        IReadOnlyCollection<long> contributionOperationTypeIds,
        int affiliateId,
        int objectiveId,
        CancellationToken cancellationToken = default)
    {
        if (!TryPrepareContributionTypeIds(contributionOperationTypeIds, out var contributionTypeIdSet))
        {
            return Array.Empty<ClientOperation>();
        }

        if (affiliateId <= 0 || objectiveId <= 0)
        {
            return Array.Empty<ClientOperation>();
        }

        return await BuildContributionOperationsQuery(contributionTypeIdSet, affiliateId, objectiveId)
            .ToListAsync(cancellationToken);
    }

    private static (DateTime InclusiveStart, DateTime ExclusiveEnd) NormalizeDateRange(DateTime startDate, DateTime endDate)
    {
        var utcStartDate = NormalizeToUtc(startDate);
        var utcEndDate = NormalizeToUtc(endDate);

        if (utcStartDate > utcEndDate)
        {
            (utcStartDate, utcEndDate) = (utcEndDate, utcStartDate);
        }

        var inclusiveStartDate = utcStartDate.Date;
        var inclusiveEndDate = utcEndDate.Date;

        return (inclusiveStartDate, inclusiveEndDate.AddDays(1));
    }

    private IQueryable<ClientOperation> BuildContributionOperationsQuery(
        long[] contributionTypeIdSet,
        int affiliateId,
        int objectiveId)
    {
        var contributionTypeIds = contributionTypeIdSet.ToList();

        return context.ClientOperations
            .AsNoTracking()
            .Include(operation => operation.AuxiliaryInformation)
            .Where(operation =>
                operation.Status == LifecycleStatus.Active &&
                operation.AffiliateId == affiliateId &&
                operation.ObjectiveId == objectiveId &&
                contributionTypeIds.Contains(operation.OperationTypeId));
    }

    private static bool TryPrepareContributionTypeIds(
        IReadOnlyCollection<long> contributionOperationTypeIds,
        out long[] contributionTypeIdSet)
    {
        if (contributionOperationTypeIds is null || contributionOperationTypeIds.Count == 0)
        {
            contributionTypeIdSet = Array.Empty<long>();
            return false;
        }

        contributionTypeIdSet = contributionOperationTypeIds
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        return contributionTypeIdSet.Length > 0;
    }

    private static DateTime NormalizeToUtc(DateTime date)
    {
        return date.Kind switch
        {
            DateTimeKind.Unspecified => DateTime.SpecifyKind(date, DateTimeKind.Utc),
            DateTimeKind.Local => date.ToUniversalTime(),
            _ => date
        };
    }
}
