using Microsoft.EntityFrameworkCore;
using Products.Domain.Alternatives;
using Products.Infrastructure.Database;

namespace Products.Infrastructure.Alternatives;

internal sealed class AlternativeRepository(ProductsDbContext context) : IAlternativeRepository
{
    public async Task<IReadOnlyCollection<Alternative>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Alternatives.ToListAsync(cancellationToken);
    }

    public async Task<Alternative?> GetAsync(int alternativeId, CancellationToken cancellationToken = default)
    {
        return await context.Alternatives
            .SingleOrDefaultAsync(x => x.AlternativeId == alternativeId, cancellationToken);
    }
    
    public async Task<Alternative?> GetByHomologatedCodeAsync(
        string homologatedCode,
        CancellationToken cancellationToken = default
    )
    {
        return await context.Alternatives
            .SingleOrDefaultAsync(x => x.HomologatedCode == homologatedCode, cancellationToken);
    }
}