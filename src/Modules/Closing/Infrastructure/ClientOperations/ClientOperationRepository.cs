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

    public async Task<bool> ClientOperationsExistsAsync(int portfolioId, DateTime closingDateUtc, long transactionSubtypeId, CancellationToken cancellationToken = default)
    {
        return await context.ClientOperations
            .AnyAsync(co => co.PortfolioId == portfolioId && co.ProcessDate == closingDateUtc && co.TransactionSubtypeId == transactionSubtypeId, cancellationToken);
    }

    public async Task<decimal> SumByPortfolioAndSubtypesAsync(
       int portfolioId,
       DateTime closingDateUtc,
       IEnumerable<long> subtransactionTypeIds,
       CancellationToken cancellationToken = default)
    {
        return await context.ClientOperations
            .Where(op => op.PortfolioId == portfolioId
                         && op.ProcessDate.Date == closingDateUtc.Date
                         && subtransactionTypeIds.Contains(op.TransactionSubtypeId))
            .SumAsync(op => (decimal?)op.Amount, cancellationToken) ?? 0m;
    }
}