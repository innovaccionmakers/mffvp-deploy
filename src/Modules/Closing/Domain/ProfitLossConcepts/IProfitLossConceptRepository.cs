namespace Closing.Domain.ProfitLossConcepts;

public interface IProfitLossConceptRepository
{
    Task<IReadOnlyCollection<ProfitLossConcept>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProfitLossConcept?> GetAsync(long profitLossConceptId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProfitLossConcept>> FindByNamesAsync(IEnumerable<string> names, CancellationToken ct = default);
    Task<ProfitLossConcept?> FindByNameAsync(string concept, CancellationToken cancellationToken = default);
}