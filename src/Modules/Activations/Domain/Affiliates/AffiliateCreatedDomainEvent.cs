using Common.SharedKernel.Domain;

namespace Activations.Domain.Affiliates;

public sealed class AffiliateCreatedDomainEvent(int affiliateId) : DomainEvent
{
    public int AffiliateId { get; } = affiliateId;
}