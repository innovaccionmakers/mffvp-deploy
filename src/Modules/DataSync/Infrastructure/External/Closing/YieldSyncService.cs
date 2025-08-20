using Closing.IntegrationEvents.DataSync.TrustSync;

using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using DataSync.Application.TrustSync;

namespace DataSync.Infrastructure.External.Closing;

internal sealed class YieldSyncService(IRpcClient rpc) : IYieldSyncService
{
    public async Task<Result> SyncTrustYieldAsync(
        int trustId,
        int portfolioId,
        DateTime closingDate,
        decimal preClosingBalance,
        decimal capital,
        decimal contingentWithholding,
        CancellationToken cancellationToken)
    {
        var response = await rpc.CallAsync<TrustSyncRequest, TrustSyncResponse>(
            new TrustSyncRequest(trustId, portfolioId, closingDate, preClosingBalance, capital, contingentWithholding),
            cancellationToken);

        return response.Succeeded
            ? Result.Success()
            : Result.Failure(Error.Validation(response.Code!, response.Message!));
    }
}
