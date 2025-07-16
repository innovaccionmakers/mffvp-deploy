using Associate.Integrations.Activates.GetActivate;
using Associate.Integrations.Activates.GetActivateId;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Presentation.Results;
using MediatR;

namespace Associate.IntegrationEvents.ActivateValidation;

public sealed class ActivateValidationConsumer : IRpcHandler<GetActivateIdByIdentificationRequest, GetActivateIdByIdentificationResponse>
{
    private readonly ISender _mediator;

    public ActivateValidationConsumer(ISender mediator) => _mediator = mediator;

    public async Task<GetActivateIdByIdentificationResponse> HandleAsync(
        GetActivateIdByIdentificationRequest message,
        CancellationToken cancellationToken)
    {
        var idResult  = await _mediator.Send(
            new GetActivateIdQuery(message.DocumentType, message.Identification),
            cancellationToken);
        
        if (!idResult.IsSuccess)
            return new GetActivateIdByIdentificationResponse(false, null, idResult.Error.Code, idResult.Error.Description);
        
        var infoResult = await _mediator.Send(
            new GetActivateQuery(idResult.Value.ActivateId),
            cancellationToken);
        
        return infoResult.Match(
            success => new GetActivateIdByIdentificationResponse(true, success, null, null),
            failure => new GetActivateIdByIdentificationResponse(false, null, failure.Error.Code, failure.Error.Description));
    }
}