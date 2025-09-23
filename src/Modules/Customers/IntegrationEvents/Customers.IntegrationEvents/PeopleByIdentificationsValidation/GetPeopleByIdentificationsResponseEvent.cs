using Customers.Integrations.People.GetPeopleByIdentifications;

namespace Customers.IntegrationEvents.PeopleByIdentificationsValidation;

public record GetPeopleByIdentificationsResponseEvent(
    bool IsValid, 
    string? Code, 
    string? Message,
    IReadOnlyCollection<GetPeopleByIdentificationsResponse>? Person);
