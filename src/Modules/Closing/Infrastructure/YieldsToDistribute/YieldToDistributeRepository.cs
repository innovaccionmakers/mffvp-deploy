using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Closing.Domain.YieldsToDistribute;
using Closing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Closing.Infrastructure.YieldsToDistribute;

internal sealed class YieldToDistributeRepository(ClosingDbContext context) : IYieldToDistributeRepository
{
    public async Task InsertRangeAsync(IEnumerable<YieldToDistribute> yieldsToDistribute, CancellationToken cancellationToken = default)
    {
        await context.Set<YieldToDistribute>()
            .AddRangeAsync(yieldsToDistribute, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<YieldToDistribute>> GetReadOnlyByPortfolioAndDateAsync(
        int portfolioId,
        DateTime closingDateUtc,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<YieldToDistribute>()
            .AsNoTracking()
            .Where(y => y.PortfolioId == portfolioId && y.ClosingDate == closingDateUtc)
            .ToListAsync(cancellationToken);
    }
}
