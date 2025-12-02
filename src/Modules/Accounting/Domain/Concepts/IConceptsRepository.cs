namespace Accounting.Domain.Concepts;

public interface IConceptsRepository
{
    Task<IEnumerable<Concept>> GetConceptsByPortfolioIdsAsync(IEnumerable<int> PortfolioIds, IEnumerable<string> Concepts, CancellationToken CancellationToken);
    Task<Concept?> GetByPortfolioIdAndNameAsync(int portfolioId, string name, CancellationToken cancellationToken = default);

    void Insert(Concept concept);
    void Update(Concept concept);
    void Delete(Concept concept);
}
