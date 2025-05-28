using Common.SharedKernel.Domain;

namespace Products.Domain.Plans;

public sealed class PlanCreatedDomainEvent(int planId) : DomainEvent
{
    public int PlanId { get; } = planId;
}