using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Customers.Integrations.People.GetPerson;

namespace Customers.IntegrationEvents.ClientValidation;

public sealed class ClientValidationConsumer : IRpcHandler<ValidatePersonByIdentificationRequest, ValidatePersonByIdentificationResponse>
{
    private readonly ISender _mediator;

    public ClientValidationConsumer(ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<ValidatePersonByIdentificationResponse> HandleAsync(
        ValidatePersonByIdentificationRequest message,
        CancellationToken cancellationToken)
    {
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