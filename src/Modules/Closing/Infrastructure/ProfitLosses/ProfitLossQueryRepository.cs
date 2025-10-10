using Closing.Domain.ProfitLosses;
using Closing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Closing.Infrastructure.ProfitLosses;

internal sealed class ProfitLossQueryRepository : IProfitLossQueryRepository
{
    private readonly IDbContextFactory<ClosingDbContext> dbFactory;

    public ProfitLossQueryRepository(IDbContextFactory<ClosingDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<ProfitLossConceptSummary>> GetReadOnlyConceptSummaryAsync(
        int portfolioId,
        DateTime effectiveDateUtc,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.ProfitLosses
            .TagWith("ProfitLossQueryRepository.GetReadOnlyConceptSummaryAsync")
            .AsNoTracking()
            .Where(pl => pl.PortfolioId == portfolioId && pl.EffectiveDate == effectiveDateUtc)
            .Join(db.ProfitLossConcepts,
                pl => pl.ProfitLossConceptId,
                c => c.ProfitLossConceptId,
                (pl, c) => new { c.ProfitLossConceptId, c.Concept, c.Nature, pl.Source, pl.Amount })
            .GroupBy(x => new { x.ProfitLossConceptId, x.Concept, x.Nature, x.Source })
            .Select(g => new ProfitLossConceptSummary(
                g.Key.ProfitLossConceptId,
                g.Key.Concept,
                g.Key.Nature,
                g.Key.Source,
                g.Sum(e => e.Amount)))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> PandLExistsAsync(
        int portfolioId,
        DateTime effectiveDateUtc,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.ProfitLosses.AsNoTracking()
            .AnyAsync(x => x.PortfolioId == portfolioId && x.EffectiveDate == effectiveDateUtc, cancellationToken);
    }

    private static DateTime EnsureUtc(DateTime dt) =>
        dt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dt, DateTimeKind.Utc) : dt.ToUniversalTime();
}
