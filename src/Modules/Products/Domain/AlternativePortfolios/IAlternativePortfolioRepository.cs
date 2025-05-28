namespace Products.Domain.AlternativePortfolios;

public interface IAlternativePortfolioRepository
{
    Task<IReadOnlyCollection<AlternativePortfolio>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AlternativePortfolio?> GetAsync(int alternativeportfolioId, CancellationToken cancellationToken = default);
    void Insert(AlternativePortfolio alternativeportfolio);
    void Update(AlternativePortfolio alternativeportfolio);
    void Delete(AlternativePortfolio alternativeportfolio);
}