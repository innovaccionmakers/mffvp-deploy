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
}