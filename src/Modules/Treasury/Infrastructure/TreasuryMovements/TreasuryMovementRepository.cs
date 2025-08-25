using Microsoft.EntityFrameworkCore;
using Common.SharedKernel.Application.Helpers.General;
using Treasury.Domain.TreasuryMovements;
using Treasury.Infrastructure.Database;

namespace Treasury.Infrastructure.TreasuryMovements;

public class TreasuryMovementRepository(TreasuryDbContext context) : ITreasuryMovementRepository
{
    public async Task<TreasuryMovement?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await context.TreasuryMovements.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(TreasuryMovement treasuryMovement, CancellationToken cancellationToken = default)
    {
        await context.TreasuryMovements.AddAsync(treasuryMovement, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TreasuryMovement>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.TreasuryMovements.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TreasuryMovementConceptSummary>> GetReadOnlyTreasuryMovementsByPortfolioAsync(int portfolioId, DateTime date, CancellationToken cancellationToken = default)
    {

        var dateUtc = DateTimeConverter.ToUtcDateTime(date);

        return await context.TreasuryMovements
            .AsNoTracking()
            .Where(pl => pl.PortfolioId == portfolioId && pl.ClosingDate == dateUtc)
            .Join(context.TreasuryConcepts,
                pl => pl.TreasuryConceptId,
                c => c.Id,
                (pl, c) => new { c.Id, c.Concept, c.Nature, c.AllowsExpense, pl.Value })
            .GroupBy(x => new { x.Id, x.Concept, x.Nature, x.AllowsExpense })
            .Select(g => new TreasuryMovementConceptSummary(
                g.Key.Id,
                g.Key.Concept,
                g.Key.Nature,
                g.Key.AllowsExpense,
                g.Sum(e => e.Value)))
            .ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<TreasuryMovement> treasuryMovements, CancellationToken cancellationToken = default)
    {
        await context.TreasuryMovements.AddRangeAsync(treasuryMovements, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TreasuryMovement>> GetTreasuryMovementsByPortfolioIdsAsync(IEnumerable<long> portfolioIds, CancellationToken cancellationToken = default)
    {
        return await context.TreasuryMovements
            .AsNoTracking()
            .Where(tm => portfolioIds.Contains(tm.PortfolioId))
            .Include(tm => tm.TreasuryConcept)
            .Include(b => b.BankAccount)
            .Include(c => c.Counterparty)
            .ToListAsync(cancellationToken);
    }
}