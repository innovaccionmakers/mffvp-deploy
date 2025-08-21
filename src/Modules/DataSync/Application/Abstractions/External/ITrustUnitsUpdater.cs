
namespace DataSync.Application.Abstractions.External.TrustSync;
public interface ITrustUnitsUpdater
{
    Task<int> UpdateUnitsAsync(int portfolioId, DateTime closingDate, CancellationToken ct);
}