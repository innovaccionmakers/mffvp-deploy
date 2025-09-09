using Microsoft.EntityFrameworkCore;
using Trusts.Domain.Trusts;
using Trusts.Domain.Trusts.Balances;
using Trusts.Domain.Trusts.TrustYield;
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

    public async Task<int> GetParticipantAsync(IEnumerable<long> trustIds, CancellationToken cancellationToken = default)
    {
        return await context.Trusts
            .AsNoTracking()
            .Where(x => trustIds.Contains(x.TrustId) && x.TotalBalance > 0)
            .Select(x => x.AffiliateId)
            .Distinct()
            .CountAsync(cancellationToken);
    }

    public async Task<int> TryApplyYieldSetBasedAsync(
     long trustId,
     decimal yieldAmount,
     decimal yieldRetention,
     decimal closingBalance,
     CancellationToken cancellationToken = default)
    {

        return await context.Trusts
            .Where(t => t.TrustId == trustId)
            //.Where(t => t.TotalBalance + yieldAmount == closingBalance)
            //.Where(t => t.TotalBalance + yieldAmount == (t.Principal + yieldAmount)) se debe validar con negocio si esta condicion se mantiene
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(t => t.TotalBalance, t => t.TotalBalance + yieldAmount)
                .SetProperty(t => t.Earnings, t => t.Earnings + yieldAmount)
                .SetProperty(t => t.EarningsWithholding, t => t.EarningsWithholding + yieldRetention)
                .SetProperty(t => t.AvailableAmount,
                    t => (t.TotalBalance + yieldAmount)
                         - (t.EarningsWithholding + yieldRetention)
                         - t.ContingentWithholding),
                cancellationToken);
    }

    public async Task<TrustYieldUpdateDiagnostics?> GetYieldUpdateDiagnosticsAsync(
      long trustId,
      decimal yieldAmount,
      decimal closingBalance,
      CancellationToken cancellationToken = default)
    {
        var data = await context.Trusts
            .AsNoTracking()
            .Where(t => t.TrustId == trustId)
            .Select(t => new
            {
                NewTotal = Math.Round(t.TotalBalance, 2) + Math.Round(yieldAmount, 2),
                ExpectedCapPlusYield = t.Principal + yieldAmount
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (data is null) return null;

        return new TrustYieldUpdateDiagnostics(
            data.NewTotal == Math.Round(closingBalance, 2),
            data.NewTotal == data.ExpectedCapPlusYield,
            data.NewTotal,
            data.ExpectedCapPlusYield
        );
    }


}