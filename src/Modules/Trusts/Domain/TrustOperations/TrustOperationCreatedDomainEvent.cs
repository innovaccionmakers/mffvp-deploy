using Common.SharedKernel.Domain;

namespace Trusts.Domain.TrustOperations;

public sealed class TrustOperationCreatedDomainEvent(Guid trustoperationId) : DomainEvent
{
    public Guid TrustOperationId { get; } = trustoperationId;
}