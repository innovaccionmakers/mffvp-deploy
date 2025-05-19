using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Plans.GetPlan;

public sealed record GetPlanQuery(
    long PlanId
) : IQuery<PlanResponse>;