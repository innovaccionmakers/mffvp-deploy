namespace Products.Domain.PortfolioValuations;

public interface IPortfolioValuationRepository
{
    Task<PortfolioValuation?> GetByPortfolioIdAsync(int portfolioId, CancellationToken cancellationToken);
    Task AddAsync(PortfolioValuation valuation, CancellationToken cancellationToken);
}
