using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.External;
using Trusts.IntegrationEvents.TrustInfo;

namespace Operations.Infrastructure.External.Trusts;

internal sealed class TrustInfoProvider(IRpcClient rpcClient) : ITrustInfoProvider
{
    public async Task<Result<TrustInfoResult>> GetAsync(
        long clientOperationId,
        decimal contributionValue,
        CancellationToken cancellationToken)
    {
        var response = await rpcClient.CallAsync<TrustInfoRequest, TrustInfoResponse>(
            new TrustInfoRequest(clientOperationId, contributionValue),
            cancellationToken);

        if (response.Succeeded && response.TrustId.HasValue)
        {
            return Result.Success(new TrustInfoResult(response.TrustId.Value));
        }

        return Result.Failure<TrustInfoResult>(
            Error.Validation(
                response.Code ?? string.Empty,
                response.Message ?? string.Empty));
    }
}
