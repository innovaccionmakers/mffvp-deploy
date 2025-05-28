using Common.SharedKernel.Domain;

namespace Products.Domain.Cities;

public sealed class CityCreatedDomainEvent(int cityId) : DomainEvent
{
    public int CityId { get; } = cityId;
}