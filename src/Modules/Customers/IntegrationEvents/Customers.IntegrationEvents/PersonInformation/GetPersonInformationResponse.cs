namespace Customers.IntegrationEvents.PersonInformation;

public sealed record GetPersonInformationResponse(
    bool IsValid,
    string? Code,
    string? Message,
    PersonInformation? Person
);