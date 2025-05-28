using Microsoft.EntityFrameworkCore;
using Products.Domain.Cities;
using Products.Infrastructure.Database;

namespace Products.Infrastructure.Cities;

internal sealed class CityRepository(ProductsDbContext context) : ICityRepository
{
    public async Task<IReadOnlyCollection<City>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Cities.ToListAsync(cancellationToken);
    }

    public async Task<City?> GetAsync(int cityId, CancellationToken cancellationToken = default)
    {
        return await context.Cities
            .SingleOrDefaultAsync(x => x.CityId == cityId, cancellationToken);
    }

    public void Insert(City city)
    {
        context.Cities.Add(city);
    }

    public void Update(City city)
    {
        context.Cities.Update(city);
    }

    public void Delete(City city)
    {
        context.Cities.Remove(city);
    }
}