namespace Products.IntegrationEvents.ContributionValidation;

public sealed record ContributionValidationResponse(
    bool    IsValid,
    string? Code    = null,
    string? Message = null);