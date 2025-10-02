namespace Treasury.Domain.TreasuryMovements;

public interface ITreasuryMovementRepository
{
    Task<TreasuryMovement?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TreasuryMovement>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TreasuryMovement treasuryMovement, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TreasuryMovement> treasuryMovements, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TreasuryMovementConceptSummary>> GetReadOnlyTreasuryMovementsByPortfolioAsync(int portfolioId, DateTime date, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TreasuryMovement>> GetTreasuryMovementsByPortfolioIdsAsync(IEnumerable<long> portfolioIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TreasuryMovement>> GetAccountingConceptsAsync(IEnumerable<int> portfolioIds, DateTime ProcessDate, CancellationToken cancellationToken = default);
}