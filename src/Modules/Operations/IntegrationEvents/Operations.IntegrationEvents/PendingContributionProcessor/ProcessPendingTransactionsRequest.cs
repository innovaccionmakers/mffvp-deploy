

namespace Operations.IntegrationEvents.PendingContributionProcessor;

public sealed record ProcessPendingTransactionsRequest(
    int PortfolioId,
    DateTime ProcessDate,
    string? IdempotencyKey = null,
    string? ExecutionId = null
);