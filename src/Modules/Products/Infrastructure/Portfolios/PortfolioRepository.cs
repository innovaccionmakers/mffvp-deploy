using Microsoft.EntityFrameworkCore;
using Products.Domain.Portfolios;
using Products.Infrastructure.Database;

namespace Products.Infrastructure.Portfolios;

internal sealed class PortfolioRepository(ProductsDbContext context) : IPortfolioRepository
{
    public async Task<IReadOnlyCollection<Portfolio>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Portfolios.ToListAsync(cancellationToken);
    }

    public async Task<Portfolio?> GetAsync(int portfolioId, CancellationToken cancellationToken = default)
    {
        return await context.Portfolios
            .SingleOrDefaultAsync(x => x.PortfolioId == portfolioId, cancellationToken);
    }

    public async Task<Portfolio?> GetByStandardCodeAsync(
        string standardCode,
        CancellationToken cancellationToken = default)
    {
        return await context.Portfolios
            .SingleOrDefaultAsync(p => p.StandardCode == standardCode,
                cancellationToken);
    }

    public Task<bool> BelongsToAlternativeAsync(
        string standardCode, int alternativeId, CancellationToken ct)
    {
        return context.AlternativePortfolios
            .AsNoTracking()
            .AnyAsync(ap =>
                    ap.AlternativeId == alternativeId
                    && ap.Portfolio.StandardCode == standardCode,
                ct);
    }

    public Task<string?> GetCollectorCodeAsync(
        int alternativeId, CancellationToken ct)
    {
        return context.AlternativePortfolios
            .AsNoTracking()
            .Where(ap => ap.AlternativeId == alternativeId
                         && ap.IsCollector)
            .Select(ap => ap.Portfolio.StandardCode)
            .FirstOrDefaultAsync(ct);
    }
}