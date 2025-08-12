using Common.SharedKernel.Domain;

namespace DataSync.Application.TrustSync;

public interface IYieldSyncService
{
    Task<Result> SyncTrustYieldAsync(
        int trustId,
        int portfolioId,
        DateTime closingDate,
        decimal preClosingBalance,
        decimal capital,
        decimal contingentWithholding,
        CancellationToken cancellationToken);
}
