using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.External;
using Trusts.IntegrationEvents.Trusts.PutTrust;

namespace Operations.Infrastructure.External.Trusts;

internal sealed class TrustUpdater(IRpcClient rpcClient) : ITrustUpdater
{
    public async Task<Result> UpdateAsync(TrustUpdate update, CancellationToken cancellationToken)
    {
        var response = await rpcClient.CallAsync<PutTrustRequest, PutTrustResponse>(
            new PutTrustRequest(
                update.ClientOperationId,
                update.Status,
                update.TotalBalance,
                update.Principal,
                update.ContingentWithholding,
                update.UpdateDate),
            cancellationToken);

        return response.Succeeded
            ? Result.Success()
            : Result.Failure(Error.Validation(
                response.Code ?? string.Empty,
                response.Message ?? string.Empty));
    }
}
