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
    
    public async Task DeleteByPortfolioAndDateAsync(int portfolioId, DateTime effectiveDate, CancellationToken cancellationToken = default)
    {
        await context.ProfitLosses
            .Where(x => x.PortfolioId == portfolioId && x.EffectiveDate == effectiveDate)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public void InsertRange(IEnumerable<ProfitLoss> profitLosses)
    {
        context.ProfitLosses.AddRange(profitLosses);
    }
}