using Common.SharedKernel.Application.Messaging;
using System;

namespace Products.Integrations.Plans.DeletePlan;
public sealed record DeletePlanCommand(
    long PlanId
) : ICommand;