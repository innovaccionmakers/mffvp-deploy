using Common.SharedKernel.Domain;

namespace Products.Domain.Alternatives;

public sealed class AlternativeCreatedDomainEvent(long alternativeId) : DomainEvent
{
    public long AlternativeId { get; } = alternativeId;
}