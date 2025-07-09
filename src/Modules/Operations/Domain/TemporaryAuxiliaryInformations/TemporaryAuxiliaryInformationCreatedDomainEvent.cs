using Common.SharedKernel.Domain;

namespace Operations.Domain.TemporaryAuxiliaryInformations;

public sealed class TemporaryAuxiliaryInformationCreatedDomainEvent(long temporaryAuxiliaryInformationId) : DomainEvent
{
    public long TemporaryAuxiliaryInformationId { get; } = temporaryAuxiliaryInformationId;
}
