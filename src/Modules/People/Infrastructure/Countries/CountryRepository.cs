using Microsoft.EntityFrameworkCore;
using People.Domain.Countries;
using People.Infrastructure.Database;

namespace People.Infrastructure.Countries;
internal sealed class CountryRepository(PeopleDbContext context) : ICountryRepository
{
    public async Task<IReadOnlyCollection<Country>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Countries.ToListAsync(cancellationToken);
    }

    public async Task<Country?> GetAsync(int countryId, CancellationToken cancellationToken = default)
    {
        return await context.Countries
            .SingleOrDefaultAsync(x => x.CountryId == countryId, cancellationToken);
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