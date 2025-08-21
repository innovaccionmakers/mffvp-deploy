namespace DataSync.Application.TrustSync.Services.Interfaces;

public interface ITrustSyncPostService
{
    Task<int> ExecuteAsync(int portfolioId, DateTime closingDate, CancellationToken ct);
}