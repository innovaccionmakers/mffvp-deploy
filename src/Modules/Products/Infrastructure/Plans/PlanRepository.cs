using Microsoft.EntityFrameworkCore;
using Products.Domain.Plans;
using Products.Infrastructure.Database;

namespace Products.Infrastructure.Plans;
internal sealed class PlanRepository(ProductsDbContext context) : IPlanRepository
{
    public async Task<IReadOnlyCollection<Plan>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Plans.ToListAsync(cancellationToken);
    }

    public async Task<Plan?> GetAsync(long planId, CancellationToken cancellationToken = default)
    {
        return await context.Plans
            .SingleOrDefaultAsync(x => x.PlanId == planId, cancellationToken);
    }

    public void Insert(Plan plan)
    {
        context.Plans.Add(plan);
    }

    public void Update(Plan plan)
    {
        context.Plans.Update(plan);
    }

    public void Delete(Plan plan)
    {
        context.Plans.Remove(plan);
    }
}