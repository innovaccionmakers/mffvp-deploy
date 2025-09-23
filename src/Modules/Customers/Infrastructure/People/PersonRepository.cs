using Associate.Domain.Activates;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Customers.Domain.People;
using Customers.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

namespace Customers.Infrastructure.People;

internal sealed class PersonRepository(CustomersDbContext context) : IPersonRepository
{
    public async Task<IReadOnlyCollection<Person>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Customers.ToListAsync(cancellationToken);
    }

    public async Task<Person?> GetAsync(long personId, CancellationToken cancellationToken = default)
    {
        return await context.Customers
            .SingleOrDefaultAsync(x => x.PersonId == personId, cancellationToken);
    }

    public void Insert(Person person)
    {
        context.Customers.Add(person);
    }

    public void Update(Person person)
    {
        context.Customers.Update(person);
    }

    public void Delete(Person person)
    {
        context.Customers.Remove(person);
    }

    public async Task<Person?> GetByIdentificationAsync(string identification, string documentTypeCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(documentTypeCode))
            return null;

        return await context.Customers.SingleOrDefaultAsync(x => x.Identification == identification, cancellationToken);
    }

    public async Task<Person?> GetForIdentificationAsync(Guid? DocumentType, string Identification, CancellationToken cancellationToken = default)
    {
        return await context.Customers.SingleOrDefaultAsync(x =>
            x.DocumentType == DocumentType && x.Identification == Identification);
    }

    public async Task<bool?> GetExistingHomologatedCode(string homologatedCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(homologatedCode))
            return null;

        return await context.Customers.AnyAsync(x => x.HomologatedCode == homologatedCode, cancellationToken);
    }

    public async Task<IReadOnlyCollection<PersonInformation>> GetActivePersonsByFilterAsync(string? identificationType,
                                                              SearchByType? searchBy = null,
                                                              string? text = null,
                                                              CancellationToken cancellationToken = default)
    {
        var query = context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(identificationType))
            query = query.Where(x => x.DocumentType.ToString() == identificationType);

        if (!string.IsNullOrWhiteSpace(text))
        {
            text = text.ToLower();

            switch (searchBy)
            {
                case SearchByType.Nombre:
                    query = query.Where(x => x.FullName.ToLower().Contains(text));
                    break;
                case SearchByType.Identificacion:
                    query = query.Where(x => x.Identification.ToLower().Contains(text));
                    break;
            }
        }

        return await (
            from person in query
            join config in context.ConfigurationParameters
                on person.DocumentType equals config.Uuid
            where person.Status == Status.Active
            select new PersonInformation(
                person.PersonId,
                person.DocumentType,
                config.HomologationCode,
                person.Identification,
                person.FullName,
                person.Status            )
        ).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Person>> GetPersonsByDocumentsAsync(IReadOnlyCollection<PersonDocumentKey> documents, CancellationToken cancellationToken = default)
    {
        if(documents == null || documents.Count == 0)
            return [];

        var keys = documents
            .Select(d => (d.DocumentTypeUuid, d.Identification))
            .ToList();

        var documentTypes = keys.Select(k => k.DocumentTypeUuid).Distinct().ToList();
        var documentNumbers = keys.Select(k => k.Identification).Distinct().ToList();

        var result = await context.Customers
            .Where(p => documentTypes.Contains(p.DocumentType) && documentNumbers.Contains(p.Identification))
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<IEnumerable<PeopleByIdentifications?>> GetPeoplebyIdentificationsAsync(IEnumerable<string> Identifications, CancellationToken cancellationToken = default)
    {
        if (Identifications == null || !Identifications.Any())
            return Enumerable.Empty<PeopleByIdentifications>();

        var identificationsSet = new HashSet<string>(Identifications);

        var query = from p in context.Customers
                    join pc in context.ConfigurationParameters
                    on p.DocumentType equals pc.Uuid
                    where identificationsSet.Contains(p.Identification)
                    select new PeopleByIdentifications(
                        p.Identification,
                        pc.Name,
                        p.FullName
                    );

        return await query.ToListAsync(cancellationToken);
    }
}