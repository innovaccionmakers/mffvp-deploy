using Operations.Domain.Channels;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Operations.Domain.Origins;
using Operations.Domain.SubtransactionTypes;
using Operations.Integrations.Contributions.CreateContribution;

namespace Operations.Application.Abstractions.Data;

public interface IContributionCatalogResolver
{
    Task<ContributionCatalogs> ResolveAsync(CreateContributionCommand cmd, CancellationToken ct);
}

public sealed record ContributionCatalogs(
    Origin? Source,
    ConfigurationParameter? OriginModality,
    ConfigurationParameter? CollectionMethod,
    ConfigurationParameter? PaymentMethod,
    Channel? Channel,
    SubtransactionType? Subtype,
    ConfigurationParameter? SubtypeCategoryCfg);