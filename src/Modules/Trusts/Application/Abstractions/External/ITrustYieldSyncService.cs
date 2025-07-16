using Common.SharedKernel.Domain;

namespace Trusts.Application.Abstractions.External;

public interface ITrustYieldSyncService
{
    Task<Result> SyncAsync(
        int trustId,
        int portfolioId,
        DateTime closingDate,
        decimal preClosingBalance,
        decimal capital,
        decimal contingentWithholding,
        CancellationToken ct);
}
