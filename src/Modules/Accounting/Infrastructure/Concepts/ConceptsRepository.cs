using Accounting.Domain.Concepts;
using Accounting.Domain.Treasuries;
using Accounting.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Infrastructure.Concepts
{
    internal class ConceptsRepository(AccountingDbContext context) : IConceptsRepository
    {
        public async Task<IEnumerable<Concept>> GetConceptsByPortfolioIdsAsync(IEnumerable<int> PortfolioIds, CancellationToken CancellationToken)
        {
            try
            {
                if (PortfolioIds == null || !PortfolioIds.Any())
                    return Enumerable.Empty<Concept>();

                var portfolioIdsSet = new HashSet<int>(PortfolioIds);

                return await context.Concepts
                    .Where(co => portfolioIdsSet.Contains(co.PortfolioId))
                    .ToListAsync(CancellationToken);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
