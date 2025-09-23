using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Presentation.Results;
using Customers.Integrations.People.GetPeopleByIdentifications;
using MediatR;

namespace Customers.IntegrationEvents.PeopleByIdentificationsValidation;

public sealed class PeopleByIdentificationsValidationConsumer(ISender _mediator) : IRpcHandler<GetPersonByIdentificationsRequestEvent, GetPeopleByIdentificationsResponseEvent>
{
    public async Task<GetPeopleByIdentificationsResponseEvent> HandleAsync(GetPersonByIdentificationsRequestEvent request,
        CancellationToken cancellationToken)
    {
        var result =
            await _mediator.Send(new GetPeopleByIdentificationsRequestQuery(request.Identifications),
                cancellationToken);

        return result.Match(
            success => new GetPeopleByIdentificationsResponseEvent(true, null, null, success),
            failure => new GetPeopleByIdentificationsResponseEvent(false, failure.Error.Code, failure.Error.Description, Array.Empty<GetPeopleByIdentificationsResponse>()));
    }
}