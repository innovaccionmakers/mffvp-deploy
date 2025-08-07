namespace Products.Domain.Portfolios;

public interface IPortfolioRepository
{
    Task<IReadOnlyCollection<Portfolio>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Portfolio?> GetAsync(int portfolioId, CancellationToken cancellationToken = default);
    Task<Portfolio?> GetByHomologatedCodeAsync(string homologatedCode, CancellationToken cancellationToken = default);
    Task<bool> BelongsToAlternativeAsync(string homologatedCode, int alternativeId, CancellationToken ct);
    Task<string?> GetCollectorCodeAsync(int alternativeId, CancellationToken ct);
    Task<PortfolioInformation?> GetPortfolioInformationByObjectiveIdAsync(string objectiveId, CancellationToken cancellationToken);
    Task UpdateAsync(Portfolio portfolio, CancellationToken ct);
    Task<IReadOnlyCollection<Portfolio>> GetPortfoliosByIdsAsync(IEnumerable<long> portfolioIds, CancellationToken cancellationToken = default);
}