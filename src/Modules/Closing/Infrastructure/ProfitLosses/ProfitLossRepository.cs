using Closing.Domain.ProfitLosses;
using Closing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Closing.Infrastructure.ProfitLosses;

internal sealed class ProfitLossRepository(ClosingDbContext context) : IProfitLossRepository
{
    public async Task<IReadOnlyCollection<ProfitLoss>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.ProfitLosses.ToListAsync(cancellationToken);
    }

    public async Task<ProfitLoss?> GetAsync(long profitLossId, CancellationToken cancellationToken = default)
    {
        return await context.ProfitLosses
            .SingleOrDefaultAsync(x => x.ProfitLossId == profitLossId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ProfitLoss>> GetByPortfolioAndDateAsync(int portfolioId,
        DateTime effectiveDate, CancellationToken cancellationToken = default)
    {
        var effectiveDateUtc = EnsureUtcDateTime(effectiveDate);

        return await context.ProfitLosses
            .AsNoTracking()
            .Where(x => x.PortfolioId == portfolioId && x.EffectiveDate == effectiveDateUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteByPortfolioAndDateAsync(int portfolioId, DateTime effectiveDate,
        CancellationToken cancellationToken = default)
    {
        var effectiveDateUtc = EnsureUtcDateTime(effectiveDate);

        await context.ProfitLosses
            .Where(x => x.PortfolioId == portfolioId && x.EffectiveDate == effectiveDateUtc)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProfitLossSummary>> GetSummaryAsync(int portfolioId, DateTime effectiveDate,
        CancellationToken cancellationToken = default)
    {
        var effectiveDateUtc = EnsureUtcDateTime(effectiveDate);

        return await context.ProfitLosses
            .AsNoTracking()
            .Where(pl => pl.PortfolioId == portfolioId && pl.EffectiveDate == effectiveDateUtc)
            .Join(context.ProfitLossConcepts,
                pl => pl.ProfitLossConceptId,
                c => c.ProfitLossConceptId,
                (pl, c) => new { c.Concept, c.Nature, pl.Amount })
            .GroupBy(x => new { x.Concept, x.Nature })
            .Select(g => new ProfitLossSummary(
                g.Key.Concept,
                g.Key.Nature,
                g.Sum(e => e.Amount)))
            .ToListAsync(cancellationToken);
    }

    public void InsertRange(IEnumerable<ProfitLoss> profitLosses)
    {
        context.ProfitLosses.AddRange(profitLosses);
    }
    private static DateTime EnsureUtcDateTime(DateTime dateTime)
    {
        return dateTime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
            : dateTime.ToUniversalTime();
    }

}