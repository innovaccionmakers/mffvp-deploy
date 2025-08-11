using Common.SharedKernel.Domain;

namespace DataSync.Application.TrustSync;

public interface ITrustDataService
{
    Task<Result<IReadOnlyCollection<TrustSyncData>>> GetActiveTrustsByPortfolioAsync(
        int portfolioId, 
        CancellationToken cancellationToken);
}

public sealed record TrustSyncData(
    int TrustId,
    int PortfolioId,
    decimal TotalBalance,
    decimal Principal,
    decimal ContingentWithholding);
