using Associate.Integrations.Activates.GetActivateIds;
using Associate.IntegrationsEvents.GetActivateIds;
using Common.SharedKernel.Application.Rpc;
using MediatR;

namespace Associate.IntegrationEvents.GetActivateIds;

public sealed class ActivateByActivateIdsValidationConsumer(ISender _mediator) : IRpcHandler<GetIdentificationByActivateIdsRequestEvent, GetIdentificationByActivateIdsResponseEvent>
{
    public async Task<GetIdentificationByActivateIdsResponseEvent> HandleAsync(
        GetIdentificationByActivateIdsRequestEvent request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetIdentificationByActivateIdsRequestQuery(request.AffiliateIds), cancellationToken);        

        return result.Match(
            success => new GetIdentificationByActivateIdsResponseEvent(true, null, null, success),
            failure => new GetIdentificationByActivateIdsResponseEvent(false, failure.Code, failure.Description, Array.Empty<GetIdentificationByActivateIdsResponse>()));
    }
}