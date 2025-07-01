using Associate.Presentation.GraphQL;
using Common.SharedKernel.Domain;
using Customers.Presentation.DTOs;
using Customers.Presentation.GraphQL;
using MFFVP.BFF.DTOs;

namespace MFFVP.BFF.Services;

public class ExperienceOrchestrator(IAssociatesExperienceQueries associatesQueries, ICustomersExperienceQueries customersQueries)
{

    public async Task<IReadOnlyCollection<AffiliateDto>> GetAllAssociatesAsync(string identificationType, SearchByType? searchBy, string? text, CancellationToken cancellationToken = default)
    {
        var persons = await customersQueries.GetPersonsByFilter(identificationType, searchBy, text, cancellationToken);
        var associates = await associatesQueries.GetAllAssociatesAsync(cancellationToken);

        var associatePensioners = associates
            .ToDictionary(a => a.Identification);


        var filteredPersons = persons
            .Where(p => associatePensioners.ContainsKey(p.Identification))
            .Select(p =>
            {
                var associate = associatePensioners[p.Identification];

                return new AffiliateDto(
                    p.FullName,
                    p.Identification,
                    p.IdentificationType,
                    associate.Id,
                    associate.Pensioner
                );
            }).ToList(); 

        return filteredPersons;
    }

}
