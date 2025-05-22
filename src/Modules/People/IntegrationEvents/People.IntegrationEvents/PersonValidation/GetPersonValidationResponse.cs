namespace People.IntegrationEvents.PersonValidation;

public record GetPersonValidationResponse(bool IsValid, string? Code, string? Message);