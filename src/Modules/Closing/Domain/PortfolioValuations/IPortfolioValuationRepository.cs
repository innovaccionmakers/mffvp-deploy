

namespace Closing.Domain.PortfolioValuations
{
    public interface IPortfolioValuationRepository
    {
        Task<PortfolioValuation?> GetValuationAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken = default);

        Task<bool> ValuationExistsAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default);

        Task<bool> ExistsByClosingDateAsync(DateTime closingDateUtc, CancellationToken cancellationToken = default);
    }
}
