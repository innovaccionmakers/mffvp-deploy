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
}
