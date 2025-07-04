using Common.SharedKernel.Domain;
using Customers.Domain.People;
using Customers.Integrations.People.GetPerson;
using Customers.Integrations.People.GetPersons;
using Customers.Presentation.DTOs;
using MediatR;

namespace Customers.Presentation.GraphQL;

public class CustomersExperienceQueries(IMediator mediator) : ICustomersExperienceQueries
{
    public async Task<IReadOnlyCollection<PersonDto>> GetPersonsByFilter(string identificationType, SearchByType? searchBy = null, string? text = null, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetPersonsByFilterQuery(identificationType, searchBy, text), cancellationToken);

        if(!result.IsSuccess) return [];

        var persons = result.Value;

        return persons.Select(x => new PersonDto(
            x.PersonId,
            x.FullName,
            x.Identification,
            x.DocumentType
        )).ToList();
    }

    public async Task<PersonDto?> GetPersonByIdentification(string documentType, string identification, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPersonForIdentificationQuery(documentType, identification), cancellationToken);

        if (!result.IsSuccess) return null;

        var person = result.Value;

        return new PersonDto(
            person.PersonId,
            person.FullName,
            person.Identification,
            person.DocumentType
        );
    }

    public async Task<IReadOnlyCollection<PersonDto>> GetPersonsByDocuments(IReadOnlyCollection<PersonDocumentKey> documents, CancellationToken cancellationToken = default)
    {
        var result = (await mediator.Send(new GetPersonsByDocumentsQuery(documents), cancellationToken)).Value;
        return result.Select(x => new PersonDto(
            x.PersonId,
            x.FullName,
            x.Identification,
            x.DocumentType
        )).ToList();
    }
}
