using Closing.IntegrationEvents.TrustSync;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Application.Abstractions.External;

namespace Trusts.Infrastructure.External.Closing;

internal sealed class TrustYieldSyncService(ICapRpcClient rpc) : ITrustYieldSyncService
{
    public async Task<Result> SyncAsync(
        int trustId,
        int portfolioId,
        DateTime closingDate,
        decimal preClosingBalance,
        decimal capital,
        decimal contingentWithholding,
        CancellationToken ct)
    {
        var rsp = await rpc.CallAsync<TrustSyncRequest, TrustSyncResponse>(
            nameof(TrustSyncRequest),
            new TrustSyncRequest(trustId, portfolioId, closingDate, preClosingBalance, capital, contingentWithholding),
            TimeSpan.FromSeconds(5),
            ct);

        return rsp.Succeeded
            ? Result.Success()
            : Result.Failure(Error.Validation(rsp.Code!, rsp.Message!));
    }
}
