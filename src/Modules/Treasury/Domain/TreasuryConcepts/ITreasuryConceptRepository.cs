using Treasury.Domain.TreasuryConcepts;

namespace Treasury.Domain.TreasuryConcepts;

public interface ITreasuryConceptRepository
{
    Task<TreasuryConcept?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TreasuryConcept>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TreasuryConcept treasuryConcept, CancellationToken cancellationToken = default);
}