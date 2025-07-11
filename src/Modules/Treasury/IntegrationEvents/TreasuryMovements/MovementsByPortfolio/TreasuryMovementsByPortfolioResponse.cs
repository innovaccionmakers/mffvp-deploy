using Treasury.Integrations.TreasuryMovements.Response;

namespace Treasury.IntegrationEvents.TreasuryMovements.TreasuryMovementsByPortfolio;

public sealed record TreasuryMovementsByPortfolioResponse
(bool Succeeded,
    string? Code,
    string? Message,
    IReadOnlyCollection<GetMovementsByPortfolioIdResponse> movements
);