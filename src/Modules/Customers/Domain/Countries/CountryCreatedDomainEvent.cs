using Common.SharedKernel.Domain;

namespace Customers.Domain.Countries;
public sealed class CountryCreatedDomainEvent(int countryId) : DomainEvent
{
    public int CountryId { get; } = countryId;
}