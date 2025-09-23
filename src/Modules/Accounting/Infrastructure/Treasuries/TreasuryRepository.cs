using Accounting.Domain.Treasuries;
using Accounting.Infrastructure.Database;
using Associate.Domain.Activates;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Infrastructure.Treasuries
{
    internal class TreasuryRepository(AccountingDbContext context) : ITreasuryRepository
    {
        public async Task<IEnumerable<Treasury>> GetTreasuriesByPortfolioIdsAsync(IEnumerable<int> PortfolioIds, CancellationToken CancellationToken)
        {
            if (PortfolioIds == null || !PortfolioIds.Any())
                return Enumerable.Empty<Treasury>();

            var portfolioIdsSet = new HashSet<int>(PortfolioIds);

            return await context.Treasuries
                .Where(co => portfolioIdsSet.Contains(co.PortfolioId))
                .ToListAsync(CancellationToken);
        }
    }
}
