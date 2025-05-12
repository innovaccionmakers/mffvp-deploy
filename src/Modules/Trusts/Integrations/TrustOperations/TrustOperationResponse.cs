namespace Trusts.Integrations.TrustOperations;

public sealed record TrustOperationResponse(
    Guid TrustOperationId,
    Guid CustomerDealId,
    Guid TrustId,
    decimal Amount
);