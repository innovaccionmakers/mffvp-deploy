using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Products.Domain.Administrators;
using Products.Infrastructure.Database;

namespace Products.Infrastructure.Administrators;

internal sealed class AdministratorRepository(ProductsDbContext context) : IAdministratorRepository
{
    public Task<bool> ExistsByEntityCodeAsync(string entityCode, CancellationToken cancellationToken = default)
    {
        return context.Administrators
            .AsNoTracking()
            .AnyAsync(a => a.EntityCode == entityCode, cancellationToken);
    }

    public Task<Administrator?> GetFirstOrderedByIdAsync(CancellationToken cancellationToken = default)
    {
        return context.Administrators
            .AsNoTracking()
            .OrderBy(a => a.AdministratorId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

