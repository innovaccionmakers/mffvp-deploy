namespace Closing.Domain.Yields;

public interface IYieldRepository
{
    Task InsertAsync(Yield yield, CancellationToken ct = default);
    Task DeleteByPortfolioAndDateAsync(
    int portfolioId,
    DateTime closingDateLocal,
    CancellationToken cancellationToken = default);
    Task<bool> ExistsYieldAsync(int portfolioId, DateTime closingDateUtc, bool isClosed, CancellationToken cancellationToken = default);

    Task<Yield?> GetByPortfolioAndDateAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default);
}
