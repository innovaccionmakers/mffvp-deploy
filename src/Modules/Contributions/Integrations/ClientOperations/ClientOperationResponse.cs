namespace Contributions.Integrations.ClientOperations;

public sealed record ClientOperationResponse(
    Guid ClientOperationId,
    DateTime Date,
    int AffiliateId,
    int ObjectiveId,
    int PortfolioId,
    int TransactionTypeId,
    int SubTransactionTypeId,
    decimal Amount
);