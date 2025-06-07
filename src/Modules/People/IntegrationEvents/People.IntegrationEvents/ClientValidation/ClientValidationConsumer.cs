using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Presentation.Results;
using DotNetCore.CAP;
using MediatR;
using People.Integrations.People.GetPerson;

namespace People.IntegrationEvents.ClientValidation;

public sealed class ClientValidationConsumer : ICapSubscribe
{
    private readonly ISender _mediator;

    public ClientValidationConsumer(ISender mediator)
    {
        _mediator = mediator;
    }

    [CapSubscribe(nameof(ValidatePersonByIdentificationRequest))]
    public async Task<ValidatePersonByIdentificationResponse> ValidateAsync(
        ValidatePersonByIdentificationRequest message,
        [FromCap] CapHeader header,
        CancellationToken cancellationToken)
    {
        var corr = header[CapRpcClient.Headers.CorrelationId];
        header.AddResponseHeader(CapRpcClient.Headers.CorrelationId, corr);

        var result =
            await _mediator.Send(
                new ValidatePersonQuery(message.DocumentTypeHomologatedCode, message.IdentificationNumber),
                cancellationToken);
        return result.Match(
            person =>
                new ValidatePersonByIdentificationResponse(
                    true,
                    null,
                    null),
            err =>
                new ValidatePersonByIdentificationResponse(
                    false,
                    err.Error.Code,
                    err.Error.Description));
    }
}