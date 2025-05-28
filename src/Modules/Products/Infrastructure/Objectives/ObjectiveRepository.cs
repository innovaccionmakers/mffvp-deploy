using Microsoft.EntityFrameworkCore;
using Products.Domain.Objectives;
using Products.Infrastructure.Database;

namespace Products.Infrastructure.Objectives;

internal sealed class ObjectiveRepository(ProductsDbContext context) : IObjectiveRepository
{
    public async Task<IReadOnlyCollection<Objective>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Objectives.ToListAsync(cancellationToken);
    }

    public async Task<Objective?> GetAsync(int objectiveId, CancellationToken cancellationToken = default)
    {
        return await context.Objectives
            .SingleOrDefaultAsync(x => x.ObjectiveId == objectiveId, cancellationToken);
    }

    public async Task<Objective?> GetByIdAsync(int objectiveId, CancellationToken ct)
    {
        return await context.Objectives
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.ObjectiveId == objectiveId, ct);
    }
}