namespace Customers.IntegrationEvents.PersonValidation;

public record GetPersonValidationRequest(long PersonId);

public sealed record PersonDataRequestEvent(
    string IdentificationType,
    string Identification
);