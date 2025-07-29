using Microsoft.EntityFrameworkCore;
using Trusts.Domain.Trusts;
using Trusts.Domain.Trusts.Balances;
using Trusts.Infrastructure.Database;

namespace Trusts.Infrastructure.Trusts;

internal sealed class TrustRepository(TrustsDbContext context) : ITrustRepository
{
    public async Task<IReadOnlyCollection<Trust>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Trusts.ToListAsync(cancellationToken);
    }

    public async Task<Trust?> GetAsync(long trustId, CancellationToken cancellationToken = default)
    {
        return await context.Trusts
            .SingleOrDefaultAsync(x => x.TrustId == trustId, cancellationToken);
    }

    public void Insert(Trust trust)
    {
        context.Trusts.Add(trust);
    }

    public void Update(Trust trust)
    {
        context.Trusts.Update(trust);
    }

    public void Delete(Trust trust)
    {
        context.Trusts.Remove(trust);
    }

    public async Task<IReadOnlyCollection<AffiliateBalance>> GetBalancesAsync(
        int affiliateId,
        CancellationToken cancellationToken = default)
    {
        return await context.Trusts
            .AsNoTracking()
            .Where(t => t.AffiliateId == affiliateId)
            .GroupBy(t => new { t.ObjectiveId, t.PortfolioId })
            .Select(g => AffiliateBalance.Create(
                g.Key.ObjectiveId,
                g.Key.PortfolioId,
                g.Sum(x => x.TotalBalance),
                g.Sum(x => x.AvailableAmount)))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Trust>> GetByObjectiveIdAsync(int objectiveId, CancellationToken cancellationToken = default)
    {
        return await context.Trusts
            .AsNoTracking()
            .Where(t => t.ObjectiveId == objectiveId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Trust>> GetActiveTrustsByPortfolioAsync(int portfolioId, CancellationToken ct)
    {
        return await context
            .Set<Trust>()
            .AsNoTracking()
            .Where(t => t.PortfolioId == portfolioId && t.Status)
            .ToListAsync(ct);
    }
}