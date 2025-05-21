namespace People.Integrations.People.GetPersonValidation;

public record GetPersonValidationResponse(bool IsValid, string? Code, string? Message);