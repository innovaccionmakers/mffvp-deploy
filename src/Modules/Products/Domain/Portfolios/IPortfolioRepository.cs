namespace Products.Domain.Portfolios;

public interface IPortfolioRepository
{
    Task<IReadOnlyCollection<Portfolio>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Portfolio?> GetAsync(int portfolioId, CancellationToken cancellationToken = default);
    Task<Portfolio?> GetByHomologatedCodeAsync(string homologatedCode, CancellationToken cancellationToken = default);
    Task<bool> BelongsToAlternativeAsync(string homologatedCode, int alternativeId, CancellationToken ct);
    Task<string?> GetCollectorCodeAsync(int alternativeId, CancellationToken ct);
}