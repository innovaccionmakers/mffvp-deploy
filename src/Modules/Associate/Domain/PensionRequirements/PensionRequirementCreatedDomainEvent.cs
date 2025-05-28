using Common.SharedKernel.Domain;

namespace Associate.Domain.PensionRequirements;
public sealed class PensionRequirementCreatedDomainEvent(int pensionrequirementId) : DomainEvent
{
    public int PensionRequirementId { get; } = pensionrequirementId;
}