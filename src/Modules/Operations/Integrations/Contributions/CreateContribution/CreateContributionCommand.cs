using System.Text.Json;
using Common.SharedKernel.Application.Messaging;

namespace Operations.Integrations.Contributions.CreateContribution;

public sealed record CreateContributionCommand(
    string TypeId,
    string Identification,
    int ObjectiveId,
    string? PortfolioId,
    decimal Amount,
    string Origin,
    string OriginModality,
    string CollectionMethod,
    string PaymentMethod,
    JsonDocument? PaymentMethodDetail,
    string CollectionBank,
    string CollectionAccount,
    string? CertifiedContribution,
    decimal? ContingentWithholding,
    DateTime DepositDate,
    DateTime ExecutionDate,
    string SalesUser,
    JsonDocument? VerifiableMedium
) : ICommand<ContributionResponse>;