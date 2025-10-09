

namespace Closing.Application.PostClosing.Services.TrustYield;

public sealed class TrustYieldRpcOptions
{
    public int MaxDegreeOfParallelism { get; init; } = 8;

    public bool UseEmitFilter { get; init; } = true;
}
