namespace Closing.Domain.Yields;

public interface IYieldRepository
{
    Task InsertAsync(Yield yield, CancellationToken ct = default);
    Task DeleteByPortfolioAndDateAsync(
    int portfolioId,
    DateTime closingDateLocal,
    CancellationToken cancellationToken = default);
    Task<bool> ExistsClosedYieldAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default);
}
