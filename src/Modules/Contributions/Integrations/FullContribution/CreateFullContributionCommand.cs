using Common.SharedKernel.Application.Messaging;

namespace Contributions.Integrations.FullContribution
{
    public sealed record CreateFullContributionCommand(
    string IdentificationType,
    string Identification,
    int ObjectiveId,
    string? PortfolioCode,
    decimal Amount,
    string OriginCode,
    string CollectionMethodCode,
    string PaymentMethodCode,
    string? CertifiedContribution,
    decimal? ContingentWithholding,
    DateTime ExecutionDate
) : ICommand<FullContributionResponse>;
}
