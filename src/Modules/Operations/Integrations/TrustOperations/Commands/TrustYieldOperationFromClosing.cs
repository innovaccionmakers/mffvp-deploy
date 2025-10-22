namespace Operations.Integrations.TrustOperations.Commands;

public sealed record TrustYieldOperationFromClosing(
    long TrustId,
    long OperationTypeId,
    decimal Amount,
    long? ClientOperationId,
    DateTime ProcessDateUtc
);