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

    public async Task<Portfolio?> GetByHomologatedCodeAsync(
        string homologatedCode,
        CancellationToken cancellationToken = default)
    {
        return await context.Portfolios
            .SingleOrDefaultAsync(p => p.HomologatedCode == homologatedCode,
                cancellationToken);
    }

    public Task<bool> BelongsToAlternativeAsync(
        string homologatedCode, int alternativeId, CancellationToken ct)
    {
        return context.AlternativePortfolios
            .AsNoTracking()
            .AnyAsync(ap =>
                    ap.AlternativeId == alternativeId
                    && ap.Portfolio.HomologatedCode == homologatedCode,
                ct);
    }

    public Task<string?> GetCollectorCodeAsync(
        int alternativeId, CancellationToken ct)
    {
        return context.AlternativePortfolios
            .AsNoTracking()
            .Where(ap => ap.AlternativeId == alternativeId
                         && ap.IsCollector)
            .Select(ap => ap.Portfolio.HomologatedCode)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PortfolioInformation?> GetPortfolioInformationByObjectiveIdAsync(string objectiveId, CancellationToken cancellationToken)
    {
        var result = await(from o in context.Objectives
                           join a in context.Alternatives on o.AlternativeId equals a.AlternativeId
                           join pf in context.PlanFunds on a.PlanFundId equals pf.PlanId
                           join pl in context.Plans on pf.PlanId equals pl.PlanId
                           join f in context.PensionFunds on pf.PensionFundId equals f.PensionFundId
                           join ap in context.AlternativePortfolios on a.AlternativeId equals ap.AlternativeId
                           join p in context.Portfolios on ap.PortfolioId equals p.PortfolioId
                           where o.ObjectiveId.ToString() == objectiveId && ap.IsCollector
                           select PortfolioInformation.Create(
                                f.Name,
                                f.PensionFundId,
                                pl.Name,
                                pl.PlanId,
                                a.Name,
                                a.AlternativeId,
                                p.Name,
                                p.PortfolioId
                               )).FirstOrDefaultAsync(cancellationToken);

        return result;
    }
}