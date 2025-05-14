using Common.SharedKernel.Domain;

namespace Trusts.Domain.Trusts;

public sealed class TrustCreatedDomainEvent(long trustId) : DomainEvent
{
    public long TrustId { get; } = trustId;
}