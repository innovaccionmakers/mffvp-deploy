using Products.Domain.Portfolios;

namespace Products.Domain.PlanFunds;

public class PlanFundQueryResponse
{
    public string PlanId { get; set; }
    public string PlanName { get; set; }
    public string HomologatedCodePlan { get; set; }
    public string FundId { get; set; }
    public string FundName { get; set; }
    public string HomologatedCodeFund { get; set; }

    public PlanFundQueryResponse()
    {

    }

    public static PlanFundQueryResponse Create(
        string planId,
        string planName,
        string homologatedCodePlan,
        string fundId,
        string fundName,
        string homologatedCodeFund)
    {
        return new PlanFundQueryResponse
        {
            PlanId = planId,
            PlanName = planName,
            HomologatedCodePlan = homologatedCodePlan,
            FundId = fundId,
            FundName = fundName,
            HomologatedCodeFund = homologatedCodeFund
        };
    }
}
