using Microsoft.EntityFrameworkCore;
using Closing.Domain.ProfitLosses;
using Closing.Infrastructure.Database;

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
}