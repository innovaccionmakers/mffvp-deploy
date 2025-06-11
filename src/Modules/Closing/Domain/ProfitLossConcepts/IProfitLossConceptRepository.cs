namespace Closing.Domain.ProfitLossConcepts;

public interface IProfitLossConceptRepository
{
    Task<IReadOnlyCollection<ProfitLossConcept>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProfitLossConcept?> GetAsync(long profitLossConceptId, CancellationToken cancellationToken = default);
}