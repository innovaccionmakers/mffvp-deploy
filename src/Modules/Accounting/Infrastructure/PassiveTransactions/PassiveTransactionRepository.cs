using Accounting.Domain.PassiveTransactions;
using Accounting.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Accounting.Infrastructure.PassiveTransactions;

public class PassiveTransactionRepository(AccountingDbContext context) : IPassiveTransactionRepository
{
    public async Task<PassiveTransaction?> GetByPortfolioIdAsync(int portfolioId, CancellationToken cancellationToken)
    {
        return await context.PassiveTransactions.SingleOrDefaultAsync(
            pt => pt.PortfolioId == portfolioId,
            cancellationToken
        );
    }

    public async Task<IEnumerable<PassiveTransaction?>> GetAccountingOperationsAsync(IEnumerable<int> PortfolioIds, IEnumerable<long> TypeOperationsIds, CancellationToken cancellationToken = default)
    {
        try
        {
            if (PortfolioIds == null || !PortfolioIds.Any())
                return Enumerable.Empty<PassiveTransaction>();

            var portfolioIdsSet = new HashSet<int>(PortfolioIds);

            return await context.PassiveTransactions
                .Where(co => portfolioIdsSet.Contains(co.PortfolioId) && TypeOperationsIds.Contains(co.TypeOperationsId))
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {

            throw;
        }
    }
}
