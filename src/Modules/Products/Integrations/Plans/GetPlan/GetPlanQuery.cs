using Common.SharedKernel.Application.Messaging;
using System;

namespace Products.Integrations.Plans.GetPlan;
public sealed record GetPlanQuery(
    long PlanId
) : IQuery<PlanResponse>;