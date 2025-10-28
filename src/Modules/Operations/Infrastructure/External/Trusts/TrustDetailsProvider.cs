using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.External;
using Trusts.IntegrationEvents.Trusts.GetTrustById;

namespace Operations.Infrastructure.External.Trusts;

internal sealed class TrustDetailsProvider(IRpcClient rpcClient) : ITrustDetailsProvider
{
    public async Task<Result<TrustDetailsResult>> GetAsync(long trustId, CancellationToken cancellationToken)
    {
        var response = await rpcClient.CallAsync<GetTrustByIdRequest, GetTrustByIdResponse>(
            new GetTrustByIdRequest(trustId),
            cancellationToken);

        if (response.Succeeded && response.Trust is not null)
        {
            return Result.Success(new TrustDetailsResult(response.Trust.Earnings));
        }

        return Result.Failure<TrustDetailsResult>(
            Error.Validation(
                response.Code ?? string.Empty,
                response.Message ?? string.Empty));
    }
}