using Associate.Presentation.GraphQL;
using Common.SharedKernel.Domain;
using Customers.Presentation.DTOs;
using Customers.Presentation.GraphQL;

namespace MFFVP.BFF.Services;

public class ExperienceOrchestrator(IAssociatesExperienceQueries associatesQueries, ICustomersExperienceQueries customersQueries)
{

    public async Task<IReadOnlyCollection<PeopleDto>> GetAllAssociatesAsync(string identificationType, SearchByType? searchBy, string? text, CancellationToken cancellationToken = default)
    {
        var persons = await customersQueries.GetPersonsByFilter(identificationType, searchBy, text, cancellationToken);
        var associates = await associatesQueries.GetAllAssociatesAsync(cancellationToken);

        var associateIdentifications = associates
            .Select(a => a.Identification)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var filteredPersons = persons
            .Where(p => associateIdentifications.Contains(p.Identication))
            .ToList();

        return filteredPersons;
    }

}
