namespace Products.Domain.Portfolios;
public interface IPortfolioRepository
{
    Task<IReadOnlyCollection<Portfolio>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Portfolio?> GetAsync(long portfolioId, CancellationToken cancellationToken = default);
    void Insert(Portfolio portfolio);
    void Update(Portfolio portfolio);
    void Delete(Portfolio portfolio);
}