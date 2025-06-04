using Microsoft.EntityFrameworkCore;
using Products.Domain.Commercials;
using Products.Infrastructure.Database;

namespace Products.Infrastructure.Commercials;

internal sealed class CommercialRepository(ProductsDbContext context) : ICommercialRepository
{
    public Task<Commercial?> GetByHomologatedCodeAsync(
        string code, CancellationToken cancellationToken = default)
    {
        return context.Commercials
            .FirstOrDefaultAsync(c => c.HomologatedCode == code, cancellationToken);
    }
}