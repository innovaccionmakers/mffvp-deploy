using Closing.Domain.TreasuryMovements;

namespace Closing.Application.PreClosing.Services.TreasuryConcepts;

public interface IMovementsConsolidationService
{
    Task<IReadOnlyList<TreasuryMovementSummary>> GetMovementsSummaryAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken);
}
