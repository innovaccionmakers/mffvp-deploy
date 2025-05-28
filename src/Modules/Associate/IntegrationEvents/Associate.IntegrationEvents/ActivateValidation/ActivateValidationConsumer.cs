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

        var result = await _mediator.Send(
            new GetActivateIdQuery(message.IdentificationType, message.Identification),
            cancellationToken);

        return result.Match(
            success  => new GetActivateIdByIdentificationResponse(true,  success.ActivateId, null, null),
            failure  => new GetActivateIdByIdentificationResponse(false, null, failure.Error.Code, failure.Error.Description));
    }
}