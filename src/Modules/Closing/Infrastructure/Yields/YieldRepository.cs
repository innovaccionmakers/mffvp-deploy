
using Closing.Domain.Yields;
using Closing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Closing.Infrastructure.Yields;
internal sealed class YieldRepository(ClosingDbContext context) : IYieldRepository
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
        var deletedCount = await context.Yields
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
        await context.Yields
            .Where(yield => yield.PortfolioId == portfolioId
                            && yield.ClosingDate == closingDateUtc
                            && yield.IsClosed)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<bool> ExistsYieldAsync(int portfolioId, DateTime closingDateUtc, bool isClosed, CancellationToken cancellationToken = default)
    {

        return await context.Yields
            .AnyAsync(y => y.PortfolioId == portfolioId
                        && y.ClosingDate == closingDateUtc
                        && y.IsClosed == isClosed,
                      cancellationToken);
    }

    public async Task<Yield?> GetByPortfolioAndDateAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default)
    {
        return await context.Yields
            .Where(y => y.PortfolioId == portfolioId && y.ClosingDate.Date == closingDateUtc.Date)
            .SingleOrDefaultAsync(cancellationToken);
    }

}
