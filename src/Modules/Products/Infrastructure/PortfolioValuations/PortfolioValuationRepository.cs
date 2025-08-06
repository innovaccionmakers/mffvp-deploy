using Microsoft.EntityFrameworkCore;
using Products.Domain.PortfolioValuations;
using Products.Infrastructure.Database;

namespace Products.Infrastructure.PortfolioValuations;

internal sealed class PortfolioValuationRepository(ProductsDbContext context) : IPortfolioValuationRepository
{
    public async Task<PortfolioValuation?> GetByPortfolioIdAsync(int portfolioId, CancellationToken cancellationToken)
    {
        return await context.PortfolioValuations
            .FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, cancellationToken);
    }

    public async Task AddAsync(PortfolioValuation valuation, CancellationToken cancellationToken)
    {
        await context.PortfolioValuations.AddAsync(valuation, cancellationToken);
    }

    public Task UpdateAsync(PortfolioValuation valuation, CancellationToken cancellationToken)
    {
        context.PortfolioValuations.Update(valuation);
        return Task.CompletedTask;
    }
}