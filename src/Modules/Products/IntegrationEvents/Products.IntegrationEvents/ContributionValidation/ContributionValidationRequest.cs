namespace Products.IntegrationEvents.ContributionValidation;

public sealed record ContributionValidationRequest(
    int      ActivateId,
    int      ObjectiveId,
    string?  PortfolioStandardCode,
    DateTime DepositDate,
    DateTime ExecutionDate,
    decimal  Amount);