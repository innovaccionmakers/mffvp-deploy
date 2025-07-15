
namespace Treasury.IntegrationEvents.TreasuryMovements.TreasuryMovementsByPortfolio;

public sealed record TreasuryMovementsByPortfolioRequest
(int PortfolioId, 
DateTime ClosingDate
);