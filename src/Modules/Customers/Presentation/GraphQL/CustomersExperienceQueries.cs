using Common.SharedKernel.Domain;
using Customers.Integrations.People.GetPersons;
using Customers.Presentation.DTOs;
using MediatR;

namespace Customers.Presentation.GraphQL;

public class CustomersExperienceQueries(IMediator mediator) : ICustomersExperienceQueries
{
    public async Task<IReadOnlyCollection<PeopleDto>> GetPersonsByFilter(string identificationType, SearchByType? searchBy = null, string? text = null, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetPersonsByFilterQuery(identificationType, searchBy, text), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve persons information.");
        }

        var persons = result.Value;

        return persons.Select(x => new PeopleDto(
            x.FullName,
            x.Identification,
            x.DocumentType.ToString()
        )).ToList();
    }
}
