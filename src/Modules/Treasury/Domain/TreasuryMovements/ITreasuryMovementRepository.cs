using Treasury.Domain.TreasuryMovements;

namespace Treasury.Domain.TreasuryMovements;

public interface ITreasuryMovementRepository
{
    Task<TreasuryMovement?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TreasuryMovement>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(TreasuryMovement treasuryMovement);
}