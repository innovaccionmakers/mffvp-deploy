using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Presentation.Results;
using Customers.Domain.People;
using Customers.Integrations.People.GetPerson;
using MediatR;

namespace Customers.IntegrationEvents.PersonValidation;

public sealed class PersonValidationConsumer : IRpcHandler<PersonDataRequestEvent, GetPersonValidationResponse>
{
    private readonly ISender _mediator;
    private readonly IPersonRepository _personRepository;

    public PersonValidationConsumer(ISender mediator, IPersonRepository personRepository)
    {
        _mediator = mediator;
        _personRepository = personRepository;
    }

    public async Task<GetPersonValidationResponse> HandleAsync(PersonDataRequestEvent request,
        CancellationToken cancellationToken)
    {
        var result =
            await _mediator.Send(new GetPersonForIdentificationQuery(request.DocumentType, request.Identification),
                cancellationToken);

        return result.Match(
            _ => new GetPersonValidationResponse(true, null, null),
            err => new GetPersonValidationResponse(false, err.Error.Code, err.Error.Description)
        );
    }
}