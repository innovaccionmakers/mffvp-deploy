using Common.SharedKernel.Domain;

namespace Closing.Application.Abstractions.External.TreasuryMovements;

public interface ITreasuryMovementsLocator
{
    Task<Result<IReadOnlyCollection<GetMovementsByPortfolioIdResponse>>> GetMovementsByPortfolioAsync(
       int portfolioId,
       DateTime closingDate,
       CancellationToken cancellationToken);
}

public sealed record TreasuryMovementsByPortfolioRequest
(int PortfolioId,
DateTime ClosingDate
);

public sealed record TreasuryMovementsByPortfolioResponse
(bool Succeeded,
    string? Code,
    string? Message,
    IReadOnlyCollection<GetMovementsByPortfolioIdResponse> movements
);

public sealed record GetMovementsByPortfolioIdResponse(
    long ConceptId,
    string ConceptName,
    string Nature,
    bool AllowsExpense,
    decimal TotalAmount
    );