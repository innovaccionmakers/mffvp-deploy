using Common.SharedKernel.Domain;

namespace Products.Domain.Plans;

public sealed class PlanCreatedDomainEvent(long planId) : DomainEvent
{
    public long PlanId { get; } = planId;
}