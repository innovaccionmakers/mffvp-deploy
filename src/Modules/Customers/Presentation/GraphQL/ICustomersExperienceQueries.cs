using Common.SharedKernel.Domain;
using Customers.Presentation.DTOs;

namespace Customers.Presentation.GraphQL;

public interface ICustomersExperienceQueries
{
    Task<IReadOnlyCollection<PersonDto>> GetPersonsByFilter(
        string identificationType,
        SearchByType? searchBy, 
        string? text, CancellationToken cancellationToken);
}
