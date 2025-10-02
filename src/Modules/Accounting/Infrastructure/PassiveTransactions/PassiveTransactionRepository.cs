using Accounting.Domain.PassiveTransactions;
using Accounting.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Infrastructure.PassiveTransactions;

public class PassiveTransactionRepository(AccountingDbContext context) : IPassiveTransactionRepository
{
    public async Task<PassiveTransaction?> GetByPortfolioIdAndOperationTypeAsync(int portfolioId, long operationTypeId, CancellationToken cancellationToken)
    {
        return await context.PassiveTransactions.SingleOrDefaultAsync(
            pt => pt.PortfolioId == portfolioId && pt.TypeOperationsId == operationTypeId,
            cancellationToken
        );
    }
}
