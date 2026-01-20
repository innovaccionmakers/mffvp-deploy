using Common.SharedKernel.Core.Primitives;
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
            .Where(t => t.AffiliateId == affiliateId && t.Status == LifecycleStatus.Active)
            .GroupBy(t => new { t.ObjectiveId, t.PortfolioId })
            .Select(g => AffiliateBalance.Create(
                g.Key.ObjectiveId,
                g.Key.PortfolioId,
                g.Sum(x => x.TotalBalance),
                g.Sum(x => x.AvailableAmount),
                g.Sum(x => x.ProtectedBalance),
                g.Sum(x => x.AgileWithdrawalAvailable)))
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
            .Where(t => t.PortfolioId == portfolioId && t.Status == LifecycleStatus.Active)
            .ToListAsync(ct);
    }
    
    public async Task<Trust?> GetByClientOperationIdAsync(long clientOperationId, CancellationToken cancellationToken = default)
    {
        return await context.Trusts
            .SingleOrDefaultAsync(t => t.ClientOperationId == clientOperationId, cancellationToken);
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

    public async Task<int> TryApplyYieldToBalanceAsync(
     long trustId,
     decimal yieldAmount,
     decimal yieldRetention,
     decimal closingBalance,
     CancellationToken cancellationToken = default)
    {

        return await context.Trusts
            .Where(t => t.TrustId == trustId)
            //.Where(t => t.TotalBalance + yieldAmount == closingBalance)
            //.Where(t => t.Principal + t.Earnings + yieldAmount == closingBalance)
            .TagWith("TrustRepository_TryApplyYieldToBalance")
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
}
