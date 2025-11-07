namespace Closing.Domain.Yields;

public interface IYieldRepository
{
    Task InsertAsync(Yield yield, CancellationToken ct = default);

    Task DeleteByPortfolioAndDateAsync(
    int portfolioId,
    DateTime closingDateLocal,
    CancellationToken cancellationToken = default);

    Task DeleteClosedByPortfolioAndDateAsync(
        int portfolioId,
        DateTime closingDateUtc,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsYieldAsync(int portfolioId, DateTime closingDateUtc, bool isClosed, CancellationToken cancellationToken = default);

    Task<Yield?> GetForUpdateByPortfolioAndDateAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Yield>> GetByClosingDateAsync(DateTime closingDate, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<Yield?> GetReadOnlyByPortfolioAndDateAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Yield>> GetComissionsByPortfolioIdsAndClosingDateAsync(IEnumerable<int> portfolioIds,
                                                                                    DateTime closingDate,
                                                                                    CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Yield>> GetYieldsByPortfolioIdsAndClosingDateAsync(IEnumerable<int> portfolioIds,
                                                                                   DateTime closingDate,
                                                                                   CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Yield>> GetAllAutConceptsByPortfolioIdsAndClosingDateAsync(IEnumerable<int> portfolioIds,
                                                                                   DateTime closingDate,
                                                                                   CancellationToken cancellationToken = default);
    Task UpdateCreditedYieldsAsync(int portfolioId, DateTime closingDateUtc, decimal distributedTotal, DateTime processDate, CancellationToken cancellationToken = default);

    Task<decimal?> GetYieldToCreditAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default);
    
    Task<decimal?> GetCreditedYieldsAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default);

    Task<YieldToDistributeDto?> GetReadOnlyToDistributeByPortfolioAndDateAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken);
}
