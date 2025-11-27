using Accounting.Domain.PassiveTransactions;
using Accounting.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

    public async Task<IEnumerable<PassiveTransaction>> GetByPortfolioIdsAndOperationTypeAsync(IEnumerable<int> portfolioIds, long operationTypeId, CancellationToken cancellationToken = default)
    {
        if (portfolioIds == null || !portfolioIds.Any())
            return Enumerable.Empty<PassiveTransaction>();

        var portfolioIdsSet = new HashSet<int>(portfolioIds);

        return await context.PassiveTransactions
            .Where(pt => portfolioIdsSet.Contains(pt.PortfolioId) && pt.TypeOperationsId == operationTypeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PassiveTransaction>> GetByPortfolioIdsAndOperationTypesAsync(IEnumerable<int> portfolioIds, IEnumerable<long> operationTypeIds, CancellationToken cancellationToken = default)
    {
        var portfolioIdsSet = new HashSet<int>(portfolioIds);

        var operationTypesIdsSet = new HashSet<long>(operationTypeIds);

        return await context.PassiveTransactions
            .Where(pt => portfolioIdsSet.Contains(pt.PortfolioId) && operationTypesIdsSet.Contains(pt.TypeOperationsId))            
            .ToListAsync(cancellationToken);
    }
}
