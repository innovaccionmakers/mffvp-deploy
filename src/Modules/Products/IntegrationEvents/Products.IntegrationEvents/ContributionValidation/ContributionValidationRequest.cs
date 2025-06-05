namespace Products.IntegrationEvents.ContributionValidation;

public sealed record ContributionValidationRequest(
    int ActivateId,
    int ObjectiveId,
    string? PortfolioHomologatedCode,
    DateTime DepositDate,
    DateTime ExecutionDate,
    decimal Amount);