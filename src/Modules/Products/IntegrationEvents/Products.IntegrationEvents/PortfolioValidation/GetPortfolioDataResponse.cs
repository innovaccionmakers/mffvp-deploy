namespace Products.IntegrationEvents.PortfolioValidation;

public sealed record GetPortfolioDataResponse(
    bool IsValid,
    string? Code,
    string? Message,
    DateTime? CurrentDate = null
);