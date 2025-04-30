using Common.SharedKernel.Domain;

namespace Contributions.Domain.TrustOperations;
public sealed class TrustOperationCreatedDomainEvent(Guid trustoperationId) : DomainEvent
{
    public Guid TrustOperationId { get; } = trustoperationId;
}