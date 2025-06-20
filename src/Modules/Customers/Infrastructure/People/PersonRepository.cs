using Microsoft.EntityFrameworkCore;
using Customers.Domain.People;
using Customers.Infrastructure.Database;

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
}