namespace Customers.IntegrationEvents.PeopleByIdentificationsValidation;

public sealed record GetPersonByIdentificationsRequestEvent(
    IEnumerable<string> Identifications
);