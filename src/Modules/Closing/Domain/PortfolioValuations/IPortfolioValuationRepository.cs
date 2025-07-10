

namespace Closing.Domain.PortfolioValuations
{
    public interface IPortfolioValuationRepository
    {
        Task<PortfolioValuation?> GetValuationAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken = default);
    }
}
