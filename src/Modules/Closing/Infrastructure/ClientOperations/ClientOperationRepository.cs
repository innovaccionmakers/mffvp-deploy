using Closing.Domain.ClientOperations;
using Closing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Closing.Infrastructure.ClientOperations;

internal sealed class ClientOperationRepository(ClosingDbContext context) : IClientOperationRepository
{
    public void Insert(ClientOperation clientOperation)
    {
        context.ClientOperations.Add(clientOperation);
    }

    public void Update(ClientOperation clientOperation)
    {
        context.ClientOperations.Update(clientOperation);
    }
    public async Task<bool> ClientOperationsExistsAsync(int portfolioId, DateTime closingDateUtc, long operationTypeId, CancellationToken cancellationToken = default)
    {
        return await context.ClientOperations.AsNoTracking().TagWith("ClientOperationRepository_ClientOperationsExistsAsync")
            .AnyAsync(co => co.PortfolioId == portfolioId && co.ProcessDate == closingDateUtc && co.OperationTypeId == operationTypeId, cancellationToken);
    }

    public async Task<decimal> SumByPortfolioAndSubtypesAsync(
       int portfolioId,
       DateTime closingDateUtc,
       IEnumerable<long> subtransactionTypeIds,
       CancellationToken cancellationToken = default)
    {
        return await context.ClientOperations.AsNoTracking().TagWith("ClientOperationRepository_SumByPortfolioAndSubtypesAsync")
            .Where(op => op.PortfolioId == portfolioId
                         && op.ProcessDate.Date == closingDateUtc.Date
                         && subtransactionTypeIds.Contains(op.OperationTypeId))
            .SumAsync(op => (decimal?)op.Amount, cancellationToken) ?? 0m;
    }
    public async Task<ClientOperation?> GetForUpdateByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await context.ClientOperations
            .Where(co => co.ClientOperationId == id)
            .TagWith("ClientOperationRepository_GetForUpdateByIdAsync")
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }
}