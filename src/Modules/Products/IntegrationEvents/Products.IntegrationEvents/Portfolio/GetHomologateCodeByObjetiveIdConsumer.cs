using Common.SharedKernel.Application.Rpc;
using MediatR;
using Products.Integrations.Objectives.Queries;

namespace Products.IntegrationEvents.Portfolio;

public class GetHomologateCodeByObjetiveIdConsumer(ISender sender) : IRpcHandler<GetHomologateCodeByObjetiveIdRequest, GetHomologateCodeByObjetiveIdResponse>
{
    public async Task<GetHomologateCodeByObjetiveIdResponse> HandleAsync(GetHomologateCodeByObjetiveIdRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new GetHomologateCodeByObjetiveIdQuery(request.ObjetiveId), ct);
        if (!result.IsSuccess)
            return new GetHomologateCodeByObjetiveIdResponse(false, "", result.Error.Code, result.Error.Description);
        return new GetHomologateCodeByObjetiveIdResponse(true, result.Value, null, null);
    }
}
