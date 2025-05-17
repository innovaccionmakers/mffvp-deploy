using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Plans.GetPlans;

public sealed record GetPlansQuery : IQuery<IReadOnlyCollection<PlanResponse>>;