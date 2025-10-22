namespace Trusts.Domain.Trusts.TrustYield;

public sealed record ApplyYieldBulkResult(
    int Updated,
    IReadOnlyCollection<long> MissingTrustIds,
    IReadOnlyCollection<long> ValidationMismatchTrustIds
);