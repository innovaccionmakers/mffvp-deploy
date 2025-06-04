using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Presentation.Results;
using DotNetCore.CAP;
using Integrations.People.GetPerson;
using MediatR;
using People.Domain.People;
using People.Integrations.People.GetPerson;

namespace People.IntegrationEvents.PersonValidation;

public sealed class PersonValidationConsumer : ICapSubscribe
{
    private readonly ISender _mediator;
    private readonly IPersonRepository _personRepository;

    public PersonValidationConsumer(ISender mediator, IPersonRepository personRepository)
    {
        _mediator = mediator;
        _personRepository = personRepository;
    }

    [CapSubscribe(nameof(PersonDataRequestEvent))]
    public async Task<GetPersonValidationResponse> HandleRequest(PersonDataRequestEvent request, [FromCap] CapHeader header, CancellationToken cancellationToken)
    {
        var corr = header[CapRpcClient.Headers.CorrelationId];
        header.AddResponseHeader(CapRpcClient.Headers.CorrelationId, corr);

        var result = await _mediator.Send(new GetPersonForIdentificationQuery(request.DocumentType, request.Identification), cancellationToken);

        return result.Match(
            _ => new GetPersonValidationResponse(true, null, null),
            err => new GetPersonValidationResponse(false, err.Error.Code, err.Error.Description)
        );
    }
}
