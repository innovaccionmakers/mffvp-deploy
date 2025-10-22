

namespace Operations.Domain.TrustOperations;

public sealed record TrustYieldOpRowForBulk(
    long TrustId,
    long OperationTypeId,
    decimal Amount,
    long? ClientOperationId,
    DateTime ProcessDateUtc
);