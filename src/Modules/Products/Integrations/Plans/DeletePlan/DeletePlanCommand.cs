using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Plans.DeletePlan;

public sealed record DeletePlanCommand(
    long PlanId
) : ICommand;