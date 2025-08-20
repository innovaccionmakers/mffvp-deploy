

namespace Closing.Domain.PortfolioValuations
{
    public interface IPortfolioValuationRepository
    {
        Task<PortfolioValuation?> GetValuationAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken = default);

        Task<bool> ValuationExistsAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default);

        Task<bool> ExistsByPortfolioIdAsync(long portfolioId, CancellationToken cancellationToken = default);
        
        Task DeleteClosedByPortfolioAndDateAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default);

        Task AddAsync(PortfolioValuation valuation, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<PortfolioValuation>> GetPortfolioValuationsByClosingDateAsync(DateTime closingDate, CancellationToken cancellationToken = default);
    }
}
