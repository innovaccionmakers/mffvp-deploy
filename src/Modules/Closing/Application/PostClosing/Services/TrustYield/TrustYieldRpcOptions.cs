

namespace Closing.Application.PostClosing.Services.TrustYield;

public sealed class TrustYieldRpcOptions
{
    /// <summary>Límite de concurrencia. Por defecto = CPU - 1 (mínimo 1).</summary>
    public int MaxDegreeOfParallelism { get; init; } = Math.Max(1, Environment.ProcessorCount - 1);

    public bool UseEmitFilter { get; init; } = true;
}
