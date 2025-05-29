using Microsoft.EntityFrameworkCore;
using Products.Domain.AlternativePortfolios;
using Products.Infrastructure.Database;

namespace Products.Infrastructure.AlternativePortfolios;

internal sealed class AlternativePortfolioRepository(ProductsDbContext context) : IAlternativePortfolioRepository
{
    public async Task<IReadOnlyCollection<AlternativePortfolio>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await context.AlternativePortfolios.ToListAsync(cancellationToken);
    }

    public async Task<AlternativePortfolio?> GetAsync(int alternativeportfolioId,
        CancellationToken cancellationToken = default)
    {
        return await context.AlternativePortfolios
            .SingleOrDefaultAsync(x => x.AlternativePortfolioId == alternativeportfolioId, cancellationToken);
    }

    public void Insert(AlternativePortfolio alternativeportfolio)
    {
        context.AlternativePortfolios.Add(alternativeportfolio);
    }

    public void Update(AlternativePortfolio alternativeportfolio)
    {
        context.AlternativePortfolios.Update(alternativeportfolio);
    }

    public void Delete(AlternativePortfolio alternativeportfolio)
    {
        context.AlternativePortfolios.Remove(alternativeportfolio);
    }
}