namespace DataSync.Application.TrustSync.Services.Interfaces;

public interface ITrustSyncClosingService
{
    Task<int> ExecuteAsync(int portfolioId, DateTime closingDate, CancellationToken ct);
}
