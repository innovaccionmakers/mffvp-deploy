namespace Trusts.Integrations.TrustYields.Commands;

public sealed record ApplyYieldBulkBatchResult(
    int BatchIndex,
    int Updated,
    IReadOnlyCollection<long> MissingTrustIds,
    IReadOnlyCollection<long> ValidationMismatchTrustIds
);