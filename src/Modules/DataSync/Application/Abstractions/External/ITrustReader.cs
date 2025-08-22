
using DataSync.Application.TrustSync.Dto;
namespace DataSync.Application.Abstractions.External.TrustSync;

public interface ITrustReader
{
    Task<IReadOnlyList<TrustRow>> ReadActiveAsync(int portfolioId, DateTime closingDate, CancellationToken ct);
}