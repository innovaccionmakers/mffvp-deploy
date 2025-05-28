using Common.SharedKernel.Domain;

namespace Products.Domain.Offices;

public sealed class OfficeCreatedDomainEvent(int officeId) : DomainEvent
{
    public int OfficeId { get; } = officeId;
}