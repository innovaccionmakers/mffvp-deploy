using Common.SharedKernel.Domain;

namespace Products.Domain.Alternatives;

public sealed class AlternativeCreatedDomainEvent(int alternativeId) : DomainEvent
{
    public int AlternativeId { get; } = alternativeId;
}