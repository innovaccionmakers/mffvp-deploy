
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
        await using var db = await dbFactory
            .CreateDbContextAsync(cancellationToken);

        var deletedCount = await db.Yields
           .TagWith("YieldRepository_DeleteByPortfolioAndDateAsync")
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
            .TagWith("YieldRepository_DeleteClosedByPortfolioAndDateAsync")
            .Where(yield => yield.PortfolioId == portfolioId
                            && yield.ClosingDate == closingDateUtc
                            && yield.IsClosed)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<bool> ExistsYieldAsync(int portfolioId, DateTime closingDateUtc, bool isClosed, CancellationToken cancellationToken = default)
    {

        return await context.Yields
            .AsNoTracking()
            .TagWith("YieldRepository_ExistsYieldAsync")
            .AnyAsync(y => y.PortfolioId == portfolioId
                        && y.ClosingDate == closingDateUtc
                        && y.IsClosed == isClosed,
                      cancellationToken);
    }

    public async Task<Yield?> GetForUpdateByPortfolioAndDateAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default)
    {
        return await context.Yields
            .TagWith("YieldRepository_GetForUpdateByPortfolioAndDateAsync")
            .Where(y => y.PortfolioId == portfolioId && y.ClosingDate.Date == closingDateUtc)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateCreditedYieldsAsync(int portfolioId, DateTime closingDateUtc, decimal distributedTotal, DateTime processDate, CancellationToken cancellationToken = default)
    {
        await context.Yields
        .Where(y => y.PortfolioId == portfolioId && y.ClosingDate == closingDateUtc)
        .TagWith("YieldRepository_UpdateCreditedYieldsAsync")
        .ExecuteUpdateAsync(setters => setters
            .SetProperty(y => y.CreditedYields, distributedTotal)
            .SetProperty(y => y.ProcessDate, processDate),
        cancellationToken);
    }

    public async Task<Yield?> GetReadOnlyByPortfolioAndDateAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default)
    {
        return await context.Yields
            .AsNoTracking()
             .TagWith("YieldRepository_GetReadOnlyByPortfolioAndDateAsync")
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

    public async Task<IReadOnlyCollection<Yield>> GetComissionsByPortfolioIdsAndClosingDateAsync(IEnumerable<int> portfolioIds,
                                                                                                 DateTime closingDate,
                                                                                                 CancellationToken cancellationToken = default)
    {
        return await context.Yields
            .AsNoTracking()
            .Where(y => portfolioIds.Contains(y.PortfolioId)
                && y.ClosingDate == closingDate
                && y.IsClosed
                && y.Commissions != 0)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Yield>> GetYieldsByPortfolioIdsAndClosingDateAsync(IEnumerable<int> portfolioIds,
                                                                                             DateTime closingDate,
                                                                                             CancellationToken cancellationToken = default)
    {
        return await context.Yields
            .AsNoTracking()
            .Where(y => portfolioIds.Contains(y.PortfolioId)
                && y.ClosingDate == closingDate
                && y.IsClosed
                && y.YieldToCredit != 0)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal?> GetYieldToCreditAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default)
    {
        return await context.Yields
            .TagWith("YieldRepository_GetForUpdateByPortfolioAndDateAsync")
            .Where(y => y.PortfolioId == portfolioId && y.ClosingDate.Date == closingDateUtc)
            .Select(y => y.YieldToCredit)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<YieldToDistribute?> GetReadOnlyToDistributeByPortfolioAndDateAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken)
    {
        return await context.Yields
            .AsNoTracking()
            .TagWith("YieldRepository_GetToDistributeByPortfolioAndDateAsync")
            .Where(r => r.PortfolioId == portfolioId &&
                       r.ClosingDate.Date == closingDateUtc)
            .Select(r => new YieldToDistribute(
                r.YieldToCredit, r.Income, r.Expenses, r.Commissions, r.Costs))
            .FirstOrDefaultAsync(cancellationToken);
    }
}

