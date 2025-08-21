using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions.External.Trusts;
using Trusts.IntegrationEvents.TrustYields;

namespace Products.Infrastructure.External.Trusts;

public class TrustYieldLocator(IRpcClient rpcClien) : ITrustYieldLocator
{
    public async Task<Result<int>> GetParticipant(IEnumerable<long> trustIds, CancellationToken cancellationToken = default)
    {
        var response = await rpcClien.CallAsync<GetTrustParticipantRequest, GetTrustParticipantResponse>(
            new GetTrustParticipantRequest(trustIds), cancellationToken);

        return response.IsValid
            ? Result.Success(response.Participants)
            : Result.Failure<int>(Error.Validation(response.Code, response.Message));

    }
}
