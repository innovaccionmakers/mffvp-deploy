using Common.SharedKernel.Domain;

namespace People.Domain.Countries;
public sealed class CountryCreatedDomainEvent(int countryId) : DomainEvent
{
    public int CountryId { get; } = countryId;
}