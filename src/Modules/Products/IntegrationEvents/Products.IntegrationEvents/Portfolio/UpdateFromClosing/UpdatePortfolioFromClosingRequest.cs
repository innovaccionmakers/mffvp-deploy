

namespace Products.IntegrationEvents.Portfolio.UpdateFromClosing;

public sealed record UpdatePortfolioFromClosingRequest(
    int PortfolioId,
    DateTime ClosingDate,
    string IdempotencyKey,
    string Origin,
    string? ExecutionId = null
);