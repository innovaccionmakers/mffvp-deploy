using Common.SharedKernel.Application.Rpc;
using MediatR;
using Products.Integrations.AdditionalInformation;
using Common.SharedKernel.Domain;

namespace Products.IntegrationEvents.AdditionalInformation;

public sealed class GetAdditionalInformationConsumer : IRpcHandler<GetAdditionalInformationRequest, GetAdditionalInformationResponse>
{
    private readonly ISender _mediator;

    public GetAdditionalInformationConsumer(ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<GetAdditionalInformationResponse> HandleAsync(
        GetAdditionalInformationRequest message,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAdditionalInformationQuery(message.AffiliateId, message.Pairs), cancellationToken);

        return result.Match(
            info => new GetAdditionalInformationResponse(true, null, null, info),
            err => new GetAdditionalInformationResponse(false, err.Code, err.Description, Array.Empty<AdditionalInformationItem>()));
    }
}
