namespace Treasury.Domain.TreasuryMovements;

public interface ITreasuryMovementRepository
{
    Task<TreasuryMovement?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TreasuryMovement>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TreasuryMovement treasuryMovement, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TreasuryMovementConceptSummary>> GetTreasuryMovementsByPortfolioAsync(int portfolioId, DateTime date, CancellationToken cancellationToken = default);
}