using Common.SharedKernel.Domain;

namespace Products.Domain.Commercials;

public sealed class CommercialCreatedDomainEvent(int commercialId) : DomainEvent
{
    public int CommercialId { get; } = commercialId;
}