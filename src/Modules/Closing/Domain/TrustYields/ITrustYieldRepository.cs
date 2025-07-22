namespace Closing.Domain.TrustYields;

public interface ITrustYieldRepository
{
    Task<TrustYield?> GetByTrustAndDateAsync(int trustId, DateTime closingDateUtc, CancellationToken cancellationToken = default);
    Task InsertAsync(TrustYield trustYield, CancellationToken cancellationToken = default);
    void Update(TrustYield trustYield);
    Task<IReadOnlyCollection<TrustYield>> GetByPortfolioAndDateAsync(int portfolioId, DateTime closingDate, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct = default);
}

