using Accounting.Domain.Concepts;
using Accounting.Domain.Treasuries;
using Accounting.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Infrastructure.Concepts
{
    internal class ConceptsRepository(AccountingDbContext context) : IConceptsRepository
    {
        public async Task<IEnumerable<Concept>> GetConceptsByPortfolioIdsAsync(IEnumerable<int> PortfolioIds, IEnumerable<string> Concepts, CancellationToken CancellationToken)
        {
            try
            {
                if (PortfolioIds == null || !PortfolioIds.Any())
                    return Enumerable.Empty<Concept>();

                var portfolioIdsSet = new HashSet<int>(PortfolioIds);

                return await context.Concepts
                    .Where(co => portfolioIdsSet.Contains(co.PortfolioId) && Concepts.Contains(co.Name))
                    .ToListAsync(CancellationToken);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<Concept?> GetByIdAsync(long conceptId, CancellationToken cancellationToken = default)
        {
            return await context.Concepts.SingleOrDefaultAsync(
                c => c.ConceptId == conceptId,
                cancellationToken
            );
        }

        public void Insert(Concept concept) => context.Concepts.Add(concept);

        public void Update(Concept concept) => context.Concepts.Update(concept);

        public void Delete(Concept concept) => context.Concepts.Remove(concept);
    }
}
