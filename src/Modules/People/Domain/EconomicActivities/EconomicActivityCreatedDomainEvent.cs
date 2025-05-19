using Common.SharedKernel.Domain;

namespace People.Domain.EconomicActivities;

public sealed class EconomicActivityCreatedDomainEvent(string economicactivityId) : DomainEvent
{
    public string EconomicActivityId { get; } = economicactivityId;
}