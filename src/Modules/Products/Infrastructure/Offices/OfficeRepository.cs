using Common.SharedKernel.Core.Primitives;

using Microsoft.EntityFrameworkCore;

using Products.Domain.Offices;
using Products.Infrastructure.Database;

namespace Products.Infrastructure.Offices;

internal sealed class OfficeRepository(ProductsDbContext context) : IOfficeRepository
{
    public async Task<IReadOnlyDictionary<string, Office>>
        GetByHomologatedCodesAsync(IEnumerable<string> codes, CancellationToken cancellationToken = default)
    {
        return await context.Offices
            .Where(o => codes.Contains(o.HomologatedCode))
            .ToDictionaryAsync(o => o.HomologatedCode, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Office>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await context.Offices
            .Where(x => x.Status == Status.Active)
            .ToListAsync(cancellationToken);
    }
}