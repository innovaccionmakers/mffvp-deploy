using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Presentation.Results;
using DotNetCore.CAP;
using MediatR;
using People.Integrations.People.GetPerson;

namespace People.IntegrationEvents.PersonValidation;

public sealed class PersonValidationConsumer : ICapSubscribe
{
    private readonly ISender _mediator;
    public PersonValidationConsumer(ISender mediator)
    {
        _mediator = mediator;
    }

    [CapSubscribe(nameof(GetPersonValidationRequest))]
    public async Task<GetPersonValidationResponse> ValidateAsync(
        GetPersonValidationRequest message,
        [FromCap] CapHeader header,
        CancellationToken cancellationToken)
    {
        var corr = header[CapRpcClient.Headers.CorrelationId];
        header.AddResponseHeader(CapRpcClient.Headers.CorrelationId, corr);

        var result = await _mediator.Send(new GetPersonQuery(message.PersonId), cancellationToken);
        return result.Match(
            _ => new GetPersonValidationResponse(true, null, null),
            err => new GetPersonValidationResponse(false, err.Error.Code, err.Error.Description)
        );
    }
}
