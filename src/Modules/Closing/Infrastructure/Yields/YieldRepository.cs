
using Closing.Domain.Yields;
using Closing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Closing.Infrastructure.Yields;
internal sealed class YieldRepository(ClosingDbContext context, IDbContextFactory<ClosingDbContext> dbFactory) : IYieldRepository
{
    public async Task InsertAsync(Yield yield, CancellationToken ct = default)
    {
        await context.Yields.AddAsync(yield, ct);
    }

    public async Task DeleteByPortfolioAndDateAsync(
    int portfolioId,
    DateTime closingDateUtc,
    CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var deletedCount = await db.Yields
            .Where(yield => yield.PortfolioId == portfolioId
                         && yield.ClosingDate == closingDateUtc
                         && !yield.IsClosed)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task DeleteClosedByPortfolioAndDateAsync(
        int portfolioId,
        DateTime closingDateUtc,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        await db.Yields
            .Where(yield => yield.PortfolioId == portfolioId
                            && yield.ClosingDate == closingDateUtc
                            && yield.IsClosed)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<bool> ExistsYieldAsync(int portfolioId, DateTime closingDateUtc, bool isClosed, CancellationToken cancellationToken = default)
    {

        return await context.Yields.AsNoTracking()
            .AnyAsync(y => y.PortfolioId == portfolioId
                        && y.ClosingDate == closingDateUtc
                        && y.IsClosed == isClosed,
                      cancellationToken);
    }

    public async Task<Yield?> GetForUpdateByPortfolioAndDateAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default)
    {
        return await context.Yields
            .Where(y => y.PortfolioId == portfolioId && y.ClosingDate.Date == closingDateUtc)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<Yield?> GetReadOnlyByPortfolioAndDateAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default)
    {
        return await context.Yields.AsNoTracking()
            .Where(y => y.PortfolioId == portfolioId && y.ClosingDate.Date == closingDateUtc)
            .SingleOrDefaultAsync(cancellationToken);
    }


    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Yield>> GetByClosingDateAsync(DateTime closingDate, CancellationToken cancellationToken = default)
    {
        return await context.Yields
            .AsNoTracking()
            .Where(y => y.ClosingDate == closingDate && y.IsClosed)
            .GroupBy(y => y.PortfolioId)
            .Select(g => g.First())
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Yield>> GetByPortfolioIdsAndClosingDateAsync(List<int> portfolioIds, DateTime closingDate, CancellationToken cancellationToken = default)
    {
        return await context.Yields
            .AsNoTracking()
            .Where(y => portfolioIds.Contains(y.PortfolioId) && y.ClosingDate == closingDate && y.IsClosed)
            .ToListAsync(cancellationToken);
    }
}
