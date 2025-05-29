using Microsoft.EntityFrameworkCore;
using Products.Domain.Offices;
using Products.Infrastructure.Database;

namespace Products.Infrastructure.Offices;

internal sealed class OfficeRepository(ProductsDbContext context) : IOfficeRepository
{
    public async Task<IReadOnlyCollection<Office>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Offices.ToListAsync(cancellationToken);
    }

    public async Task<Office?> GetAsync(int officeId, CancellationToken cancellationToken = default)
    {
        return await context.Offices
            .SingleOrDefaultAsync(x => x.OfficeId == officeId, cancellationToken);
    }

    public void Insert(Office office)
    {
        context.Offices.Add(office);
    }

    public void Update(Office office)
    {
        context.Offices.Update(office);
    }

    public void Delete(Office office)
    {
        context.Offices.Remove(office);
    }
}