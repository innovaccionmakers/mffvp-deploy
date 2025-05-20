using Common.SharedKernel.Domain;

namespace Operations.Domain.ClientOperations;

public sealed class ClientOperationCreatedDomainEvent(long clientoperationId) : DomainEvent
{
    public long ClientOperationId { get; } = clientoperationId;
}