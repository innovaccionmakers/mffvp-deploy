using Accounting.Domain.PassiveTransactions;
using Accounting.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

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
}
