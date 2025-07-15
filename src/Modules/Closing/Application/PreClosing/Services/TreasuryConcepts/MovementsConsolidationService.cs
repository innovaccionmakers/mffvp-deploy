using Closing.Application.Abstractions.External.Treasury.TreasuryMovements;
using Closing.Domain.TreasuryMovements;

namespace Closing.Application.PreClosing.Services.TreasuryConcepts
{
    public class MovementsConsolidationService : IMovementsConsolidationService
    {
        private readonly ITreasuryMovementsLocator _movementsLocator;

        public MovementsConsolidationService( ITreasuryMovementsLocator movementsLocator)
        {
            _movementsLocator = movementsLocator;
        }
        public async Task<IReadOnlyList<TreasuryMovementSummary>> GetMovementsSummaryAsync(
             int portfolioId,
             DateTime closingDate,
             CancellationToken cancellationToken)
        {
            var result = await _movementsLocator
                .GetMovementsByPortfolioAsync(portfolioId, closingDate, cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException($"Error al obtener movimientos: {result.Error?.Description}");
            }

            var summaries = result.Value
                .Select(c => new TreasuryMovementSummary(
                    c.ConceptId,
                    c.ConceptName,
                    c.Nature,
                    c.AllowsExpense,
                    c.TotalAmount))
                .ToList();

            return summaries;
        }

    }
}
