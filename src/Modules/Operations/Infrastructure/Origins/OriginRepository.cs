using Common.SharedKernel.Core.Primitives;

using Microsoft.EntityFrameworkCore;

using Operations.Domain.Origins;
using Operations.Infrastructure.Database;

namespace Operations.Infrastructure.Origins;

internal sealed class OriginRepository(OperationsDbContext context) : IOriginRepository
{
    public async Task<Origin?> FindByHomologatedCodeAsync(
        string homologatedCode,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<Origin>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                o => o.HomologatedCode == homologatedCode,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<Origin>> GetOriginsAsync(
        CancellationToken cancellationToken = default)
    {
        return await context.Set<Origin>()
            .Where(o => o.Status == Status.Active)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}