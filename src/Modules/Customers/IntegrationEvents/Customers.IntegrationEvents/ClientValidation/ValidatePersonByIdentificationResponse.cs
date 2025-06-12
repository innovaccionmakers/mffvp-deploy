namespace Customers.IntegrationEvents.ClientValidation;

public sealed record ValidatePersonByIdentificationResponse(
    bool IsValid,
    string? Code,
    string? Message);