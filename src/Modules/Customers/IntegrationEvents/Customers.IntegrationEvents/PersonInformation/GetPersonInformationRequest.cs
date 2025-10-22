namespace Customers.IntegrationEvents.PersonInformation;

public sealed record GetPersonInformationRequest(
    string DocumentType,
    string Identification
);
