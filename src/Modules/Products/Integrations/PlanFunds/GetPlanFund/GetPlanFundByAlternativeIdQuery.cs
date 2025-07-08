using Common.SharedKernel.Application.Messaging;
using Products.Domain.PlanFunds;

namespace Products.Integrations.PlanFunds.GetPlanFund;

public sealed record class GetPlanFundByAlternativeIdQuery(
    string AlternativeId
    ) : IQuery<PlanFundQueryResponse>;
