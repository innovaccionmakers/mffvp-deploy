using Associate.Presentation.GraphQL;
using Common.SharedKernel.Domain;
using Customers.Domain.People;
using Customers.Presentation.GraphQL;
using MFFVP.BFF.DTOs;

namespace MFFVP.BFF.Services;

public class ExperienceOrchestrator(IAssociatesExperienceQueries associatesQueries, ICustomersExperienceQueries customersQueries)
{
    public async Task<IReadOnlyCollection<AffiliateDto>> GetAllAssociatesAsync(CancellationToken cancellationToken = default)
    {
        var associates = await associatesQueries.GetAllAssociatesAsync(cancellationToken);

        if (associates == null || associates.Count == 0)
            return [];

        var documents = associates
            .Select(a => new PersonDocumentKey(a.DocumentTypeUuid, a.Identification))
            .ToList();

        var persons = await customersQueries.GetPersonsByDocuments(documents, cancellationToken);
        if (persons == null || persons.Count == 0)
            return [];

        var personMap = persons.ToDictionary(
           p => (p.DocumentTypeUuid, p.Identification),
           p => p);

        var result = associates
            .Where(a => personMap.ContainsKey((a.DocumentTypeUuid, a.Identification)))
            .Select(a =>
            {
                var person = personMap[(a.DocumentTypeUuid, a.Identification)];
                return new AffiliateDto(
                    person.FullName,
                    a.Identification,
                    a.DocumentType,
                    a.DocumentTypeUuid,
                    a.Id,
                    a.Pensioner,
                    a.ActivateDate);
            })
            .ToList();

        return result;
    }
    public async Task<IReadOnlyCollection<AffiliateDto>> GetAllAssociatesByFilterAsync(string identificationType, SearchByType? searchBy, string? text, CancellationToken cancellationToken = default)
    {
        var persons = await customersQueries.GetPersonsByFilter(identificationType, searchBy, text, cancellationToken);
        var associates = await associatesQueries.GetAllAssociatesAsync(cancellationToken);

        if (persons.Count == 0 || associates.Count == 0) return [];

        var associatePensioners = associates
            .ToDictionary(a => (a.DocumentTypeUuid, a.Identification));

        var filteredPersons = persons
            .Where(p => associatePensioners.ContainsKey((p.DocumentTypeUuid, p.Identification)))
            .Select(p =>
            {
                var associate = associatePensioners[(p.DocumentTypeUuid, p.Identification)];

                return new AffiliateDto(
                    p.FullName,
                    associate.Identification,
                    associate.DocumentType,
                    associate.DocumentTypeUuid,
                    associate.Id,
                    associate.Pensioner,
                    associate.ActivateDate
                );
            }).ToList();

        return filteredPersons;
    }
    public async Task<AffiliateDto?> GetAssociateByIdAsync(long affiliateId, CancellationToken cancellationToken = default)
    {
       var associate = await associatesQueries.GetAssociateByIdAsync(affiliateId, cancellationToken);

        if (associate == null) return null;

        var person = await customersQueries.GetPersonByIdentification(associate.DocumentType, associate.Identification, cancellationToken);

        if (person == null) return null;

        return new AffiliateDto(
            person.FullName,
            associate.Identification,
            associate.DocumentType,
            associate.DocumentTypeUuid,
            associate.Id,
            associate.Pensioner,
            associate.ActivateDate
        );
    }
}
