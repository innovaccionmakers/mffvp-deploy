
using DataSync.Application.TrustSync.Dto;

namespace DataSync.Application.Abstractions.External.TrustSync;

public interface IClosingTrustYieldMerger
{
    Task<int> MergeAsync(IReadOnlyList<TrustRow> rows, CancellationToken ct);
}