using Common.SharedKernel.Domain;

namespace Operations.Domain.TemporaryClientOperations;

public sealed class TemporaryClientOperationCreatedDomainEvent(long temporaryClientOperationId) : DomainEvent
{
    public long TemporaryClientOperationId { get; } = temporaryClientOperationId;
}
