using Closing.Application.Closing.Services.TrustSync;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain;
using DataSync.IntegrationEvents.TrustSync;

namespace Closing.Infrastructure.External.DataSync;

internal sealed class DataSyncTrustService(IRpcClient rpc) : IDataSyncService
{
    public async Task<Result> ExecuteAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        var response = await rpc.CallAsync<TrustSyncRequest, TrustSyncResponse>(
            new TrustSyncRequest(closingDate, portfolioId),
            cancellationToken);

        return response.Succeeded
            ? Result.Success()
            : Result.Failure(Error.Validation(response.Code!, response.Message!));
    }
}
