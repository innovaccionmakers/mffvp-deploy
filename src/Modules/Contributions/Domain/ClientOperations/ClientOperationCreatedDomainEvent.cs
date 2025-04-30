using Common.SharedKernel.Domain;

namespace Contributions.Domain.ClientOperations;
public sealed class ClientOperationCreatedDomainEvent(Guid clientoperationId) : DomainEvent
{
    public Guid ClientOperationId { get; } = clientoperationId;
}