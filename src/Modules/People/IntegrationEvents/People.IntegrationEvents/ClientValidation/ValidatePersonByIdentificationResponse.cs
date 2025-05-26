namespace People.IntegrationEvents.PersonValidation;

public sealed record ValidatePersonByIdentificationResponse(
    bool IsValid,
    string? Code,
    string? Message);