using Common.SharedKernel.Domain;

namespace Closing.Application.Abstractions.External.Treasury.TreasuryMovements;

public interface ITreasuryMovementsLocator
{
    Task<Result<IReadOnlyCollection<MovementsByPortfolioRemoteResponse>>> GetMovementsByPortfolioAsync(
       int portfolioId,
       DateTime closingDate,
       CancellationToken cancellationToken);
}

public sealed record MovementsByPortfolioRemoteResponse(
    long ConceptId,
    string ConceptName,
    string Nature,
    bool AllowsExpense,
    decimal TotalAmount
    );