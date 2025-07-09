using Microsoft.EntityFrameworkCore;
using Treasury.Domain.TreasuryMovements;
using Treasury.Infrastructure.Database;

namespace Treasury.Infrastructure.TreasuryMovements;

public class TreasuryMovementRepository(TreasuryDbContext context) : ITreasuryMovementRepository
{
    public async Task<TreasuryMovement?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await context.TreasuryMovements.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TreasuryMovement>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.TreasuryMovements.ToListAsync(cancellationToken);
    }

    public void Add(TreasuryMovement treasuryMovement)
    {
        context.TreasuryMovements.Add(treasuryMovement);
    }
}