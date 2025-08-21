

namespace Closing.Domain.PortfolioValuations
{
    public interface IPortfolioValuationRepository
    {
        Task<PortfolioValuation?> GetReadOnlyByPortfolioAndDateAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken = default);

        Task<bool> ExistsByPortfolioAndDateAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default);

        Task<bool> ExistsByPortfolioIdAsync(long portfolioId, CancellationToken cancellationToken = default);
        
        Task DeleteClosedByPortfolioAndDateAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default);

        Task InsertAsync(PortfolioValuation valuation, CancellationToken cancellationToken = default);
    }
}
