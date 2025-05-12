using Common.SharedKernel.Domain;

namespace Activations.Domain.MeetsPensionRequirements;
public sealed class MeetsPensionRequirementCreatedDomainEvent(int meetspensionrequirementId) : DomainEvent
{
    public int MeetsPensionRequirementId { get; } = meetspensionrequirementId;
}