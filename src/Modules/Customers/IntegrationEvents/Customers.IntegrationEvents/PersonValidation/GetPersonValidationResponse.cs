namespace Customers.IntegrationEvents.PersonValidation;

public record GetPersonValidationResponse(bool IsValid, string? Code, string? Message);
