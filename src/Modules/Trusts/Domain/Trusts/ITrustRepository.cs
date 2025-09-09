using Trusts.Domain.Trusts.TrustYield;

namespace Trusts.Domain.Trusts;

public interface ITrustRepository
{
    Task<IReadOnlyCollection<Trust>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Trust?> GetAsync(long trustId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Balances.AffiliateBalance>> GetBalancesAsync(int affiliateId, CancellationToken cancellationToken = default);
    void Insert(Trust trust);
    void Update(Trust trust);
    void Delete(Trust trust);
    Task<IReadOnlyCollection<Trust>> GetByObjectiveIdAsync(int objectiveId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Trust>> GetActiveTrustsByPortfolioAsync(int portfolioId, CancellationToken ct);
    Task<int> GetParticipantAsync(IEnumerable<long> trustIds, CancellationToken cancellationToken = default);
    Task<int> TryApplyYieldSetBasedAsync(
     long trustId,
     decimal yieldAmount,
     decimal yieldRetention,
     decimal closingBalance,
     CancellationToken cancellationToken = default);

    Task<TrustYieldUpdateDiagnostics?> GetYieldUpdateDiagnosticsAsync(
       long trustId,
       decimal yieldAmount,
       decimal closingBalance,
       CancellationToken cancellationToken = default);
}