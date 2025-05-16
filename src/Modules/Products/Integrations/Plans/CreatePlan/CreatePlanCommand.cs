using Common.SharedKernel.Application.Messaging;
using System;

namespace Products.Integrations.Plans.CreatePlan;
public sealed record CreatePlanCommand(
    string Name,
    string Description
) : ICommand<PlanResponse>;