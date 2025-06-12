using Common.SharedKernel.Domain;

namespace Customers.Domain.EconomicActivities;

public sealed class EconomicActivityCreatedDomainEvent(int economicactivityId) : DomainEvent
{
    public int EconomicActivityId { get; } = economicactivityId;
}