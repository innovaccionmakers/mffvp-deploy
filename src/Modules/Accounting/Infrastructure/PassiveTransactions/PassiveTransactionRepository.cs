using Accounting.Domain.PassiveTransactions;
using Accounting.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Accounting.Infrastructure.PassiveTransactions;

public class PassiveTransactionRepository(AccountingDbContext context) : IPassiveTransactionRepository
{
    public async Task<IReadOnlyCollection<PassiveTransaction>> GetByPortfolioIdAsync(int portfolioId)
    {
        return await context.PassiveTransactions
            .AsNoTracking()
            .Where(x => x.PortfolioId == portfolioId)
            .ToListAsync();
    }
}
