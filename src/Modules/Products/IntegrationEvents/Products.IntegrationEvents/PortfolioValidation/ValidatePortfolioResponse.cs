namespace Products.IntegrationEvents.PortfolioValidation;

public sealed record ValidatePortfolioResponse(bool IsValid, string? Code, string? Message);