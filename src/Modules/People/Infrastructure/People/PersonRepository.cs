using Microsoft.EntityFrameworkCore;
using People.Domain.People;
using People.Infrastructure.Database;

namespace People.Infrastructure.People;

internal sealed class PersonRepository(PeopleDbContext context) : IPersonRepository
{
    public async Task<IReadOnlyCollection<Person>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.People.ToListAsync(cancellationToken);
    }

    public async Task<Person?> GetAsync(long personId, CancellationToken cancellationToken = default)
    {
        return await context.People
            .SingleOrDefaultAsync(x => x.PersonId == personId, cancellationToken);
    }

    public void Insert(Person person)
    {
        context.People.Add(person);
    }

    public void Update(Person person)
    {
        context.People.Update(person);
    }

    public void Delete(Person person)
    {
        context.People.Remove(person);
    }

    public async Task<Person?> GetForIdentificationAsync(Guid IdentificationType, string Identification,
        CancellationToken cancellationToken = default)
    {
        return await context.People.SingleOrDefaultAsync(x =>
            x.IdentificationType == IdentificationType && x.Identification == Identification);
    }

    public async Task<Person?> GetByIdentificationAsync(string identification, string documentTypeCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(documentTypeCode))
            return null;

        return await context.People.SingleOrDefaultAsync(x => x.Identification == identification, cancellationToken);
    }
}