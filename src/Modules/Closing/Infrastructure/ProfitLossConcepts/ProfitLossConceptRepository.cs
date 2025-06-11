using Microsoft.EntityFrameworkCore;
using Closing.Domain.ProfitLossConcepts;
using Closing.Infrastructure.Database;

namespace Closing.Infrastructure.ProfitLossConcepts;

internal sealed class ProfitLossConceptRepository(ClosingDbContext context) : IProfitLossConceptRepository
{
    public async Task<IReadOnlyCollection<ProfitLossConcept>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.ProfitLossConcepts.ToListAsync(cancellationToken);
    }

    public async Task<ProfitLossConcept?> GetAsync(long profitLossConceptId, CancellationToken cancellationToken = default)
    {
        return await context.ProfitLossConcepts
            .SingleOrDefaultAsync(x => x.ProfitLossConceptId == profitLossConceptId, cancellationToken);
    }
}