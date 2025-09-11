namespace Products.IntegrationEvents.EntityValidation;

public sealed record ValidateEntityResponse(bool IsValid, string? Code, string? Message);
