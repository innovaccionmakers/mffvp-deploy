using Microsoft.EntityFrameworkCore;
using Products.Domain.Objectives;
using Products.Infrastructure.Database;

namespace Products.Infrastructure;
internal sealed class ObjectiveRepository(ProductsDbContext context) : IObjectiveRepository
{
    public async Task<IReadOnlyCollection<Objective>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Objectives.ToListAsync(cancellationToken);
    }

    public async Task<Objective?> GetAsync(long objectiveId, CancellationToken cancellationToken = default)
    {
        return await context.Objectives
            .SingleOrDefaultAsync(x => x.ObjectiveId == objectiveId, cancellationToken);
    }

    public void Insert(Objective objective)
    {
        context.Objectives.Add(objective);
    }

    public void Update(Objective objective)
    {
        context.Objectives.Update(objective);
    }

    public void Delete(Objective objective)
    {
        context.Objectives.Remove(objective);
    }
}