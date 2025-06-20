using Microsoft.EntityFrameworkCore;
using Customers.Domain.Municipalities;
using Customers.Infrastructure.Database;

namespace Customers.Infrastructure;

internal sealed class MunicipalityRepository(CustomersDbContext context) : IMunicipalityRepository
{
    public async Task<IReadOnlyCollection<Municipality>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Municipalities.ToListAsync(cancellationToken);
    }

    public async Task<Municipality?> GetAsync(string homologatedCode, CancellationToken cancellationToken = default)
    {
        return await context.Municipalities
            .SingleOrDefaultAsync(x => x.HomologatedCode == homologatedCode, cancellationToken);
    }

    public void Insert(Municipality municipality)
    {
        context.Municipalities.Add(municipality);
    }

    public void Update(Municipality municipality)
    {
        context.Municipalities.Update(municipality);
    }

    public void Delete(Municipality municipality)
    {
        context.Municipalities.Remove(municipality);
    }
}