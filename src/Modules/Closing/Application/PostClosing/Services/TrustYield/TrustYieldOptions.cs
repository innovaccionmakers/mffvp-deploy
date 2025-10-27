

namespace Closing.Application.PostClosing.Services.TrustYield;

public sealed class TrustYieldOptions
{
    public int BulkBatchSize { get; init; } = 10_000;

    public bool UseEmitFilter { get; init; } = false;

}
