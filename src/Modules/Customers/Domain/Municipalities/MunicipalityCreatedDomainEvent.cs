using Common.SharedKernel.Domain;

namespace Customers.Domain.Municipalities;
public sealed class MunicipalityCreatedDomainEvent(int municipalityId) : DomainEvent
{
    public int MunicipalityId { get; } = municipalityId;
}