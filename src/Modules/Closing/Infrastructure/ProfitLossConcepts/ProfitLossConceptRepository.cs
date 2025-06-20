using Microsoft.EntityFrameworkCore;
using Closing.Domain.ProfitLossConcepts;
using Closing.Infrastructure.Database;

namespace Closing.Infrastructure.ProfitLossConcepts;

internal sealed class ProfitLossConceptRepository(ClosingDbContext context) : IProfitLossConceptRepository
{
    public async Task<IReadOnlyCollection<ProfitLossConcept>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.ProfitLossConcepts
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<ProfitLossConcept?> GetAsync(long profitLossConceptId, CancellationToken cancellationToken = default)
    {
        return await context.ProfitLossConcepts
            .SingleOrDefaultAsync(x => x.ProfitLossConceptId == profitLossConceptId, cancellationToken);
    }
    
    public async Task<IReadOnlyCollection<ProfitLossConcept>> FindByNamesAsync(IEnumerable<string> names, CancellationToken ct = default)
    {
        var list = names.Select(n => n.Trim()).ToArray();
        return await context.ProfitLossConcepts
            .AsNoTracking()
            .Where(c => list.Contains(c.Concept))
            .ToListAsync(ct);
    }
    
    public async Task<ProfitLossConcept?> FindByNameAsync(string concept, CancellationToken cancellationToken = default)
    {
        return await context.ProfitLossConcepts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Concept == concept, cancellationToken);
    }
}