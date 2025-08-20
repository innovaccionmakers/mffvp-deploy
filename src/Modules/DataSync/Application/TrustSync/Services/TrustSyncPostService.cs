using DataSync.Application.Abstractions.External.TrustSync;
using DataSync.Application.TrustSync.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DataSync.Application.TrustSync.Services;

public sealed class TrustSyncPostService(ITrustUnitsUpdater updater, ILogger<TrustSyncPostService> logger)
    : ITrustSyncPostService
{
    public async Task<int> ExecuteAsync(int portfolioId, DateTime closingDate, CancellationToken ct)
    {
        var updated = await updater.UpdateUnitsAsync(portfolioId, closingDate.Date, ct);
        logger.LogInformation("TrustSync Post: portfolio {PortfolioId} date {Date} updated {Updated}",
                              portfolioId, closingDate, updated);
        return updated;
    }
}