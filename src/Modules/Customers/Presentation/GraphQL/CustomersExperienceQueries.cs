using Common.SharedKernel.Domain;
using Customers.Integrations.People.GetPersons;
using Customers.Presentation.DTOs;
using MediatR;

namespace Customers.Presentation.GraphQL;

public class CustomersExperienceQueries(IMediator mediator) : ICustomersExperienceQueries
{
    public async Task<IReadOnlyCollection<PersonDto>> GetPersonsByFilter(string identificationType, SearchByType? searchBy = null, string? text = null, CancellationToken cancellationToken = default)
    {
        var result = (await mediator.Send(new GetPersonsByFilterQuery(identificationType, searchBy, text), cancellationToken)).Value;
        
        return result.Select(x => new PersonDto(
            x.PersonId,
            x.FullName,
            x.Identification,
            x.DocumentType.ToString()
        )).ToList();
    }
}
