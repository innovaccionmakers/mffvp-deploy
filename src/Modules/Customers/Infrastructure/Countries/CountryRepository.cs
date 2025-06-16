using Microsoft.EntityFrameworkCore;
using Customers.Domain.Countries;
using Customers.Infrastructure.Database;

namespace Customers.Infrastructure.Countries;

internal sealed class CountryRepository(CustomersDbContext context) : ICountryRepository
{
    public async Task<IReadOnlyCollection<Country>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Countries.ToListAsync(cancellationToken);
    }

    public async Task<Country?> GetAsync(string HomologatedCode, CancellationToken cancellationToken = default)
    {
        var result = await context.Countries
            .SingleOrDefaultAsync(x => x.HomologatedCode == HomologatedCode, cancellationToken);
            
        return result;
    }

    public void Insert(Country country)
    {
        context.Countries.Add(country);
    }

    public void Update(Country country)
    {
        context.Countries.Update(country);
    }

    public void Delete(Country country)
    {
        context.Countries.Remove(country);
    }
}