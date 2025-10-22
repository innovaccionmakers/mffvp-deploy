namespace Operations.Domain.TrustOperations;

public sealed record UpsertBulkResult(
    int Inserted,
    int Updated,
    IReadOnlyCollection<long> ChangedTrustIds // insertados o actualizados
);
