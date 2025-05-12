using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.FullContribution;

public sealed record CreateFullContributionCommand(
    string IdentificationType,
    string IdentificationNumber,
    int ObjectiveId,
    string? PortfolioCode,
    decimal Amount,
    DateTime DepositDate,
    DateTime ExecutionDate,
    string OriginCode,
    string OriginMode,
    string CollectionMethodCode,
    string PaymentMethodCode,
    string? PaymentDetails,
    string CollectionBank,
    string CollectionAccount,
    string? CertificationStatus,
    int? ContingentWithholding,
    string SalesUser,
    string? VerifiableMedium
) : ICommand<FullContributionResponse>;