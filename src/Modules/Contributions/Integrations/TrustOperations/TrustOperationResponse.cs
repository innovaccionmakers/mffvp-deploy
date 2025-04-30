namespace Contributions.Integrations.TrustOperations;

public sealed record TrustOperationResponse(
    Guid TrustOperationId,
    Guid ClientOperationId,
    Guid TrustId,
    decimal Amount
);