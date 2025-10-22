using Common.SharedKernel.Core.Primitives;
using Microsoft.EntityFrameworkCore;
using Operations.Domain.ClientOperations;
using Operations.Infrastructure.Database;
using System.Linq;

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
            .Where(co => co.ProcessDate == utcProcessDate && co.Status == LifecycleStatus.Active)
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

    public async Task<IEnumerable<ClientOperation>> GetAccountingOperationsAsync(IEnumerable<int> portfolioIds, DateTime processDate, CancellationToken cancellationToken = default)
    {
        if (portfolioIds == null || !portfolioIds.Any())
            return Enumerable.Empty<ClientOperation>();

        var portfolioIdsSet = new HashSet<int>(portfolioIds);

        return await context.ClientOperations
            .Where(co => co.Status == LifecycleStatus.Active)
            .Where(co => portfolioIdsSet.Contains(co.PortfolioId) && co.ProcessDate == processDate)
            .Include(co => co.AuxiliaryInformation)
            .Include(co => co.OperationType)
            .Where(co => co.OperationType != null && (co.OperationType.OperationTypeId == 1 || co.OperationType.CategoryId == 1))
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
        if (contributionOperationTypeIds is null || contributionOperationTypeIds.Count == 0)
        {
            return Array.Empty<ClientOperation>();
        }

        if (affiliateId <= 0 || objectiveId <= 0)
        {
            return Array.Empty<ClientOperation>();
        }

        var contributionTypeIdSet = contributionOperationTypeIds
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        if (contributionTypeIdSet.Length == 0)
        {
            return Array.Empty<ClientOperation>();
        }

        var utcStartDate = NormalizeToUtc(startDate);
        var utcEndDate = NormalizeToUtc(endDate);

        if (utcStartDate > utcEndDate)
        {
            (utcStartDate, utcEndDate) = (utcEndDate, utcStartDate);
        }

        var inclusiveStartDate = utcStartDate.Date;
        var inclusiveEndDate = utcEndDate.Date;
        var exclusiveEndDate = inclusiveEndDate.AddDays(1);

        var query = context.ClientOperations
            .AsNoTracking()
            .Include(operation => operation.AuxiliaryInformation)
            .Where(operation =>
                operation.Status == LifecycleStatus.Active &&
                operation.AffiliateId == affiliateId &&
                operation.ObjectiveId == objectiveId &&
                contributionTypeIdSet.Contains(operation.OperationTypeId) &&
                operation.ProcessDate >= inclusiveStartDate &&
                operation.ProcessDate < exclusiveEndDate);

        return await query.ToListAsync(cancellationToken);
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
