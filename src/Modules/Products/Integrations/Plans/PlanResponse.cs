namespace Products.Integrations.Plans;

public sealed record PlanResponse(
    long PlanId,
    string Name,
    string Description
);