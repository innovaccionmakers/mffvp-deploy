using Common.SharedKernel.Domain;

namespace Operations.Domain.AuxiliaryInformations;

public sealed class AuxiliaryInformationCreatedDomainEvent(long auxiliaryinformationId) : DomainEvent
{
    public long AuxiliaryInformationId { get; } = auxiliaryinformationId;
}