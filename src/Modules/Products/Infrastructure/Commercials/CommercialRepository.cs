using Microsoft.EntityFrameworkCore;
using Products.Domain.Commercials;
using Products.Infrastructure.Database;

namespace Products.Infrastructure.Commercials;

internal sealed class CommercialRepository(ProductsDbContext context) : ICommercialRepository
{
    public async Task<IReadOnlyCollection<Commercial>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Commercials.ToListAsync(cancellationToken);
    }

    public async Task<Commercial?> GetAsync(int commercialId, CancellationToken cancellationToken = default)
    {
        return await context.Commercials
            .SingleOrDefaultAsync(x => x.CommercialId == commercialId, cancellationToken);
    }

    public void Insert(Commercial commercial)
    {
        context.Commercials.Add(commercial);
    }

    public void Update(Commercial commercial)
    {
        context.Commercials.Update(commercial);
    }

    public void Delete(Commercial commercial)
    {
        context.Commercials.Remove(commercial);
    }
}