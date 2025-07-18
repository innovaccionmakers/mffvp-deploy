using Closing.Application.PreClosing.Services.TreasuryConcepts.Dto;

namespace Closing.Application.PreClosing.Services.TreasuryConcepts;

public interface IMovementsConsolidationService
{
    Task<IReadOnlyList<TreasuryMovementSummary>> GetMovementsSummaryAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken);

    Task<bool> HasTreasuryMovementsAsync(
           int portfolioId,
           DateTime closingDate,
           CancellationToken cancellationToken);
}
