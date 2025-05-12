using Common.SharedKernel.Domain;

namespace Trusts.Domain.CustomerDeals;

public sealed class CustomerDealCreatedDomainEvent(Guid customerdealId) : DomainEvent
{
    public Guid CustomerDealId { get; } = customerdealId;
}