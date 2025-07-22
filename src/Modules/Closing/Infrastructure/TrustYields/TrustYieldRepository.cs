using Closing.Domain.TrustYields;
using Closing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Closing.Infrastructure.TrustYields;

internal sealed class TrustYieldRepository(ClosingDbContext context) : ITrustYieldRepository
{
    public async Task<TrustYield?> GetByTrustAndDateAsync(int trustId, DateTime closingDateUtc, CancellationToken cancellationToken = default)
    {
        return await context.TrustYields
            .SingleOrDefaultAsync(x => x.TrustId == trustId && x.ClosingDate == closingDateUtc, cancellationToken);
    }

    public async Task InsertAsync(TrustYield trustYield, CancellationToken cancellationToken = default)
    {
        await context.TrustYields.AddAsync(trustYield, cancellationToken);
    }

    public void Update(TrustYield trustYield)
    {
        context.TrustYields.Update(trustYield);
    }

    public async Task<IReadOnlyCollection<TrustYield>> GetByPortfolioAndDateAsync(int portfolioId, DateTime closingDate, CancellationToken ct)
    {
        return await context.TrustYields
            .Where(t => t.PortfolioId == portfolioId && t.ClosingDate.Date == closingDate.Date)
            .ToListAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await context.SaveChangesAsync(ct);
    }
}

