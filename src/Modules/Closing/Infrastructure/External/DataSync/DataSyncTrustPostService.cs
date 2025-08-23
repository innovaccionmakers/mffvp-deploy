
using Closing.Application.PostClosing.Services.TrustSync;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using DataSync.IntegrationEvents.TrustSync;

namespace Closing.Infrastructure.External.DataSync;

internal sealed class DataSyncTrustPostService(IRpcClient rpc) : IDataSyncPostService
{
    public async Task<Result> ExecuteAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        var response = await rpc.CallAsync<TrustSyncPostRequest, TrustSyncPostResponse>(
            new TrustSyncPostRequest(portfolioId, closingDate),
            cancellationToken);

        return response.Succeeded
            ? Result.Success()
            : Result.Failure(Error.Validation(response.Code!, response.Message!));
    }
}
