using Common.SharedKernel.Application.Rpc;
using MediatR;
using Trusts.Integrations.Trusts.Queries;

namespace Trusts.IntegrationEvents.TrustYields;

public class GetTrustParticipantConsumer(ISender sender) : IRpcHandler<GetTrustParticipantRequest, GetTrustParticipantResponse>
{
    public async Task<GetTrustParticipantResponse> HandleAsync(GetTrustParticipantRequest request, CancellationToken cancellationToken)
    {
        var result =  await sender.Send(new GetParticipantQuery(request.TrustIds), cancellationToken);

        if(!result.IsSuccess)
            return new GetTrustParticipantResponse(false, result.Error.Code, result.Error.Description, 0);

        return new GetTrustParticipantResponse(true, null, null, result.Value);
    }
}