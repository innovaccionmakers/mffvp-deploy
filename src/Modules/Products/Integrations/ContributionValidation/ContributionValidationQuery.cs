using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.ContributionValidation;

public record ContributionValidationQuery(
    int ActivateId,
    int ObjectiveId,
    string? PortfolioHomologatedCode,
    DateTime DepositDate,
    DateTime ExecutionDate,
    decimal Amount
) : IQuery<ContributionValidationResponse>;