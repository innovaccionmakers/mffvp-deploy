namespace Closing.Domain.TrustYields;

public interface ITrustYieldRepository
{
    Task InsertAsync(TrustYield trustYield, CancellationToken cancellationToken = default);
    void Update(TrustYield trustYield);
    Task<IReadOnlyCollection<TrustYield>> GetByPortfolioAndDateAsync(int portfolioId, DateTime closingDate, CancellationToken ct);
    Task<IReadOnlyCollection<PortfolioTrustIds>> GetTrustIdsByPortfolioAsync(DateTime closingDate, CancellationToken ct);
    Task<TrustYield?> GetReadOnlyByTrustAndDateAsync(long trustId, DateTime closingDateUtc, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TrustYield>> GetForUpdateByPortfolioAndDateAsync(int portfolioId, DateTime closingDate, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct = default);

    Task UpsertAsync(YieldTrustSnapshot snapshot, CancellationToken ct);
}
