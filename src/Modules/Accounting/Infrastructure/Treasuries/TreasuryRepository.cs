using Accounting.Domain.Treasuries;
using Accounting.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Infrastructure.Treasuries
{
    internal class TreasuryRepository(AccountingDbContext context) : ITreasuryRepository
    {
        public async Task<IEnumerable<Domain.Treasuries.Treasury>> GetAccountingConceptsTreasuriesAsync(IEnumerable<int> PortfolioIds, CancellationToken CancellationToken)
        {
            try
            {
                if (PortfolioIds == null || !PortfolioIds.Any())
                    return Enumerable.Empty<Domain.Treasuries.Treasury>();

                var portfolioIdsSet = new HashSet<int>(PortfolioIds);

                return await context.Treasuries
                    .Where(co => portfolioIdsSet.Contains(co.PortfolioId))
                    .ToListAsync(CancellationToken);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Domain.Treasuries.Treasury>> GetAccountingOperationsTreasuriesAsync(IEnumerable<int> PortfolioIds, IEnumerable<string> CollectionAccount, CancellationToken CancellationToken)
        {
            try
            {
                if (PortfolioIds == null || !PortfolioIds.Any())
                    return Enumerable.Empty<Domain.Treasuries.Treasury>();

                var portfolioIdsSet = new HashSet<int>(PortfolioIds);

                return await context.Treasuries
                    .Where(co => portfolioIdsSet.Contains(co.PortfolioId) && CollectionAccount.Contains(co.BankAccount))
                    .ToListAsync(CancellationToken);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
