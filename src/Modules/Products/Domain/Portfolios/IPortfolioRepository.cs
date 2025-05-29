namespace Products.Domain.Portfolios;

public interface IPortfolioRepository
{
    Task<IReadOnlyCollection<Portfolio>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Portfolio?> GetAsync(int portfolioId, CancellationToken cancellationToken = default);
    Task<Portfolio?> GetByStandardCodeAsync(string standardCode, CancellationToken cancellationToken = default);
    Task<bool> BelongsToAlternativeAsync(string standardCode, int alternativeId, CancellationToken ct);
    Task<string?> GetCollectorCodeAsync(int alternativeId, CancellationToken ct);
}