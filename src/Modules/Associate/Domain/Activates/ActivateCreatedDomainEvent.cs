using Common.SharedKernel.Domain;

namespace Associate.Domain.Activates;

public sealed class ActivateCreatedDomainEvent(int activateId) : DomainEvent
{
    public int ActivateId { get; } = activateId;
}