using Microsoft.EntityFrameworkCore;
using Treasury.Domain.TreasuryConcepts;
using Treasury.Infrastructure.Database;

namespace Treasury.Infrastructure.TreasuryConcepts;

public class TreasuryConceptRepository(TreasuryDbContext context) : ITreasuryConceptRepository
{
    public async Task<TreasuryConcept?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await context.TreasuryConcepts.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TreasuryConcept>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.TreasuryConcepts.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TreasuryConcept treasuryConcept, CancellationToken cancellationToken = default)
    {
        await context.TreasuryConcepts.AddAsync(treasuryConcept, cancellationToken);
    }
}