namespace Products.IntegrationEvents.ContributionValidation;

public sealed record ContributionValidationResponse(
    bool IsValid,
    string? Code = null,
    string? Message = null,
    int? AffiliateId = null,
    int? ObjectiveId = null,
    int? PortfolioId = null,
    decimal? PortfolioInitialMinimumAmount = null,
    decimal? PortfolioAdditionalMinimumAmount = null,
    string? PortfolioName = null
);