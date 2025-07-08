using Microsoft.EntityFrameworkCore;
using Products.Domain.PlanFunds;
using Products.Infrastructure.Database;

namespace Products.Infrastructure.PlanFunds;

internal sealed class PlanFundRepository(ProductsDbContext context) : IPlanFundRepository
{
    
    public async Task<PlanFundQueryResponse?> GetPlanFundByAlternativeIdAsync(string alternativeId, CancellationToken cancellationToken)
    {
        var result = await(from a in context.Alternatives
                           join pf in context.PlanFunds on a.PlanFundId equals pf.PlanId
                           join pl in context.Plans on pf.PlanId equals pl.PlanId
                           join f in context.PensionFunds on pf.PensionFundId equals f.PensionFundId
                           where a.AlternativeId.ToString() == alternativeId
                           select PlanFundQueryResponse.Create(
                               pl.PlanId.ToString(),
                               pl.Name,
                               pl.HomologatedCode,
                               f.PensionFundId.ToString(),
                                f.ShortName,
                                f.HomologatedCode
                               )).FirstOrDefaultAsync(cancellationToken);

        return result;
    }
}