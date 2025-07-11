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

    public async Task DeleteByPortfolioAndDateAsync(int portfolioId, DateTime date,
    CancellationToken cancellationToken = default)
    {
        var effectiveDateUtc = DateTimeConverter.ToUtcDateTime(date);

        await context.Yields
            .Where(x => x.PortfolioId == portfolioId && x.ClosingDate == effectiveDateUtc)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
