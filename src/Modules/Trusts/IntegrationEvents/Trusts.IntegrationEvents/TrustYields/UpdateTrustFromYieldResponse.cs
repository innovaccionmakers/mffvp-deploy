
namespace Trusts.IntegrationEvents.TrustYields;

public sealed record UpdateTrustFromYieldResponse(
    bool Succeeded,
    int BatchIndex,
    int Updated,
    IReadOnlyCollection<long> MissingTrustIds,
    IReadOnlyCollection<long> ValidationMismatchTrustIds,
    string? Code = null,
    string? Message = null
);