using Common.SharedKernel.Domain;

namespace Products.Domain.Objectives;
public sealed class ObjectiveCreatedDomainEvent(long objectiveId) : DomainEvent
{
    public long ObjectiveId { get; } = objectiveId;
}