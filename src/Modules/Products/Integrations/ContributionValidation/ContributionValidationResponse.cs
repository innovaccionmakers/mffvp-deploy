namespace Products.Integrations.ContributionValidation;

public sealed record ContributionValidationResponse(
    bool IsValid,
    int? AffiliateId,
    int? ObjectiveId,
    int? PortfolioId,
    decimal? PortfolioInitialMinimumAmount,
    decimal? PortfolioAdditionalMinimumAmount,
    string? PortfolioName
);