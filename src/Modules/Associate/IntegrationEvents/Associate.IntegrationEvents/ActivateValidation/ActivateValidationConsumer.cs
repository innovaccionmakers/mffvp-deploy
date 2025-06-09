using Associate.Integrations.Activates.GetActivate;
using Associate.Integrations.Activates.GetActivateId;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Presentation.Results;
using DotNetCore.CAP;
using MediatR;

namespace Associate.IntegrationEvents.ActivateValidation;

public sealed class ActivateValidationConsumer : ICapSubscribe
{
    private readonly ISender _mediator;

    public ActivateValidationConsumer(ISender mediator) => _mediator = mediator;

    [CapSubscribe(nameof(GetActivateIdByIdentificationRequest))]
    public async Task<GetActivateIdByIdentificationResponse> GetActivateIdAsync(
        GetActivateIdByIdentificationRequest message,
        [FromCap] CapHeader header,
        CancellationToken cancellationToken)
    {
        var corr = header[CapRpcClient.Headers.CorrelationId];
        header.AddResponseHeader(CapRpcClient.Headers.CorrelationId, corr);

        var idResult  = await _mediator.Send(
            new GetActivateIdQuery(message.IdentificationType, message.Identification),
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