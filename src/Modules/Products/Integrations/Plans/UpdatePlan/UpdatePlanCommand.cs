using Common.SharedKernel.Application.Messaging;
using System;

namespace Products.Integrations.Plans.UpdatePlan;
public sealed record UpdatePlanCommand(
    long PlanId,
    string NewName,
    string NewDescription
) : ICommand<PlanResponse>;