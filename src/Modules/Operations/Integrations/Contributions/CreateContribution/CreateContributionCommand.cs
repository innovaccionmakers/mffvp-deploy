using System.Text.Json;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Operations.Integrations.Contributions.CreateContribution;

public sealed record CreateContributionCommand(
    string TypeId,
    string Identification,
    int ObjectiveId,
    string? PortfolioId,
    decimal Amount,
    [property: HomologScope("Origen Aporte")]
    string Origin,
    [property: HomologScope("Modalidad Origen Aporte")]
    string OriginModality,
    [property: HomologScope("Metodo de Recaudo")]
    string CollectionMethod,
    [property: HomologScope("Metodo de Pago")]
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