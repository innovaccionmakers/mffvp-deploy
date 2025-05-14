using Microsoft.EntityFrameworkCore;
using Products.Domain.Portfolios;
using Products.Infrastructure.Database;

namespace Products.Infrastructure;
internal sealed class PortfolioRepository(ProductsDbContext context) : IPortfolioRepository
{
    public async Task<IReadOnlyCollection<Portfolio>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Portfolios.ToListAsync(cancellationToken);
    }

    public async Task<Portfolio?> GetAsync(long portfolioId, CancellationToken cancellationToken = default)
    {
        return await context.Portfolios
            .SingleOrDefaultAsync(x => x.PortfolioId == portfolioId, cancellationToken);
    }

    public void Insert(Portfolio portfolio)
    {
        context.Portfolios.Add(portfolio);
    }

    public void Update(Portfolio portfolio)
    {
        context.Portfolios.Update(portfolio);
    }

    public void Delete(Portfolio portfolio)
    {
        context.Portfolios.Remove(portfolio);
    }
}