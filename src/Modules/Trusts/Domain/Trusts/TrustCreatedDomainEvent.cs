using Common.SharedKernel.Domain;

namespace Trusts.Domain.Trusts;

public sealed class TrustCreatedDomainEvent(Guid trustId) : DomainEvent
{
    public Guid TrustId { get; } = trustId;
}