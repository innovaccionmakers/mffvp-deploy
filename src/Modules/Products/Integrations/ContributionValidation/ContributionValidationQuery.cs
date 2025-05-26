using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.ContributionValidation;

public record ContributionValidationQuery(
    int ObjectiveId,
    string? PortfolioStandardCode,
    DateTime DepositDate,
    DateTime ExecutionDate,
    decimal Amount
) : IQuery<ContributionValidationResponse>;