using Common.SharedKernel.Domain;

namespace Products.Domain.Objectives;

public sealed class ObjectiveCreatedDomainEvent(int objectiveId) : DomainEvent
{
    public int ObjectiveId { get; } = objectiveId;
}