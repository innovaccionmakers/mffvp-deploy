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
                op.PortfolioId == portfolioId)
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
            .Where(co => co.ProcessDate == utcProcessDate)
            .Include(co => co.AuxiliaryInformation)
            .Include(co => co.OperationType)
            .ToListAsync(cancellationToken);

        foreach (var operation in clientOperations)
        {
            if (operation.OperationType?.CategoryId.HasValue == true)
            {
                var categoryName = await context.OperationTypes
                    .Where(ot => ot.OperationTypeId == operation.OperationType.CategoryId.Value)
                    .Select(ot => ot.Name)
                    .FirstOrDefaultAsync(cancellationToken);

                operation.OperationType.Name = categoryName;
            }
        }

        return clientOperations;
    }
}