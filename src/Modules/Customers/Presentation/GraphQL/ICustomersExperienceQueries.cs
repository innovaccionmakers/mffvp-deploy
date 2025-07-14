using Common.SharedKernel.Domain;
using Customers.Domain.People;
using Customers.Presentation.DTOs;

namespace Customers.Presentation.GraphQL;

public interface ICustomersExperienceQueries
{
    Task<IReadOnlyCollection<PersonDto>> GetPersonsByFilter(
        string? identificationType,
        SearchByType? searchBy, 
        string? text, CancellationToken cancellationToken);

    Task<PersonDto?> GetPersonByIdentification(string documentType,
                                               string identification,
                                               CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PersonDto>> GetPersonsByDocuments(IReadOnlyCollection<PersonDocumentKey> documents,
                                                               CancellationToken cancellationToken = default);
}
