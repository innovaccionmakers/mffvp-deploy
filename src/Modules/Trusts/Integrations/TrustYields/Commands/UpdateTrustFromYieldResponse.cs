
namespace Trusts.Integrations.TrustYields.Commands;

public sealed record UpdateTrustFromYieldResponse(
    bool Succeeded,
    int BatchIndex,
    int Updated,
    IReadOnlyCollection<long> MissingTrustIds,
    IReadOnlyCollection<long> ValidationMismatchTrustIds,
    string? Code = null,
    string? Message = null
);