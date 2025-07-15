
using Closing.Domain.Yields;
using Closing.Infrastructure.Database;
using Common.SharedKernel.Domain.Utils;
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
    DateTime closingDateLocal,
    CancellationToken cancellationToken = default)
    {
        var closingDateUtc = DateTimeConverter.ToUtcDateTime(closingDateLocal);

        var deletedCount = await context.Yields
            .Where(yield => yield.PortfolioId == portfolioId
                         && yield.ClosingDate == closingDateUtc
                         && !yield.IsClosed)
            .ExecuteDeleteAsync(cancellationToken);
    }


    public async Task<bool> ExistsClosedYieldAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default)
    {

        return await context.Yields
            .AnyAsync(y => y.PortfolioId == portfolioId
                        && y.ClosingDate == closingDateUtc
                        && y.IsClosed,
                      cancellationToken);
    }

}
