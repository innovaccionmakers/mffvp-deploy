namespace Products.Domain.PlanFunds;

public interface IPlanFundRepository
{
    Task<PlanFundQueryResponse?> GetPlanFundByAlternativeIdAsync(string alternativeId, CancellationToken cancellationToken);
}
