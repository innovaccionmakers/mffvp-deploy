using Accounting.Domain.Treasuries;
using Accounting.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Infrastructure.Treasuries
{
    internal class TreasuryRepository(AccountingDbContext context) : ITreasuryRepository
    {
        public async Task<IEnumerable<Domain.Treasuries.Treasury>> GetAccountingConceptsTreasuriesAsync(IEnumerable<int> PortfolioIds, IEnumerable<string> AccountNumbers, CancellationToken CancellationToken)
        {
            try
            {
                if (PortfolioIds == null || !PortfolioIds.Any())
                    return Enumerable.Empty<Domain.Treasuries.Treasury>();

                var portfolioIdsSet = new HashSet<int>(PortfolioIds);

                return await context.Treasuries
                    .Where(co => portfolioIdsSet.Contains(co.PortfolioId) && AccountNumbers.Contains(co.BankAccount))
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

        public async Task<Domain.Treasuries.Treasury?> GetTreasuryAsync(int PortfolioId, string BankAccount, CancellationToken CancellationToken)
        {
            try
            {
                return await context.Treasuries
                    .SingleOrDefaultAsync(co => co.PortfolioId == PortfolioId && co.BankAccount == BankAccount, CancellationToken);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void Insert(Domain.Treasuries.Treasury Treasury) => context.Treasuries.Add(Treasury);

        public void Update(Domain.Treasuries.Treasury Treasury) => context.Treasuries.Update(Treasury);

        public void Delete(Domain.Treasuries.Treasury Treasury) => context.Treasuries.Remove(Treasury);

        public async Task<IReadOnlyCollection<Domain.Treasuries.Treasury>> GetTreasuriesAsync(CancellationToken CancellationToken)
        {
            return await context.Treasuries.ToListAsync(CancellationToken);
        }
    }
}
