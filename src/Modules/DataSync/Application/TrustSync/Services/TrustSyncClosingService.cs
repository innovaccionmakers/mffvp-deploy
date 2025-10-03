
using DataSync.Application.Abstractions.External.TrustSync;
using DataSync.Application.TrustSync.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DataSync.Application.TrustSync.Services;

public sealed class TrustSyncClosingService(
    ITrustReader reader,
    IClosingTrustYieldMerger merger,
    ILogger<TrustSyncClosingService> logger)
    : ITrustSyncClosingService
{
    public async Task<int> ExecuteAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        var rows = await reader.ReadActiveAsync(portfolioId, closingDate.Date, cancellationToken);
        if (rows.Count == 0) return 0;
        var affected = await merger.MergeAsync(rows, cancellationToken);
        return affected;
    }
}