using Microsoft.EntityFrameworkCore;
using Products.Application.Abstractions.Services.AdditionalInformation;
using Products.Infrastructure.Database;
using Products.Integrations.AdditionalInformation;

namespace Products.Infrastructure.AdditionalInformation;

internal sealed class AdditionalInformationService(ProductsDbContext context) : IAdditionalInformationService
{
    public async Task<IReadOnlyCollection<AdditionalInformationItem>> GetInformationAsync(
        IReadOnlyCollection<(int ObjectiveId, int PortfolioId)> pairs,
        CancellationToken ct = default)
    {
        if (pairs.Count == 0)
            return Array.Empty<AdditionalInformationItem>();

        var objectiveIds = pairs.Select(p => p.ObjectiveId).ToArray();
        var portfolioIds = pairs.Select(p => p.PortfolioId).ToArray();

        var data = await (
            from o in context.Objectives
            join a in context.Alternatives on o.AlternativeId equals a.AlternativeId
            join pf in context.PlanFunds on a.PlanFundId equals pf.PlanFundId
            join f in context.PensionFunds on pf.PensionFundId equals f.PensionFundId
            join ap in context.AlternativePortfolios on a.AlternativeId equals ap.AlternativeId
            join p in context.Portfolios on ap.PortfolioId equals p.PortfolioId
            where objectiveIds.Contains(o.ObjectiveId) && portfolioIds.Contains(ap.PortfolioId)
            select new
            {
                o.ObjectiveId,
                PortfolioId = ap.PortfolioId,
                ObjectiveName = o.Name,
                a.AlternativeId,
                AlternativeName = a.Name,
                PortfolioName = p.Name,
                FundId = f.PensionFundId,
                FundName = f.Name,
                PortfolioCode = p.HomologatedCode,
                AlternativeCode = a.HomologatedCode,
                FundCode = f.HomologatedCode
            })
            .ToListAsync(ct);

        var pairSet = pairs.ToHashSet();

        var result = data
            .Where(d => pairSet.Contains((d.ObjectiveId, d.PortfolioId)))
            .Select(d => new AdditionalInformationItem(
                d.PortfolioId,
                d.PortfolioName,
                d.ObjectiveId,
                d.ObjectiveName,
                d.AlternativeId,
                d.AlternativeName,
                d.FundId,
                d.FundName,
                d.PortfolioCode,
                d.ObjectiveId.ToString(),
                d.AlternativeCode,
                d.FundCode))
            .ToList();
        return result;
    }
}
