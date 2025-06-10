using Operations.Domain.ConfigurationParameters;
using Common.SharedKernel.Application.Attributes;
using Operations.Application.Abstractions.Data;
using Operations.Domain.Channels;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Operations.Domain.Origins;
using Operations.Domain.SubtransactionTypes;
using Operations.Integrations.Contributions.CreateContribution;

namespace Operations.Application.Contributions.Services;

public sealed class ContributionCatalogResolver(
    IOriginRepository originRepo,
    IConfigurationParameterRepository cfgRepo,
    ISubtransactionTypeRepository subtypeRepo,
    IChannelRepository channelRepo)
    : IContributionCatalogResolver
{
    public async Task<ContributionCatalogs> ResolveAsync(CreateContributionCommand cmd, CancellationToken ct)
    {
        var source = await originRepo.FindByHomologatedCodeAsync(cmd.Origin, ct);

        var scopes = new[]
        {
            (cmd.OriginModality, HomologScope.Of<CreateContributionCommand>(c => c.OriginModality)),
            (cmd.CollectionMethod, HomologScope.Of<CreateContributionCommand>(c => c.CollectionMethod)),
            (cmd.PaymentMethod, HomologScope.Of<CreateContributionCommand>(c => c.PaymentMethod))
        };

        var cfgs = await cfgRepo.GetByCodesAndTypesAsync(scopes, ct);
        var originMod = cfgs.GetValueOrDefault(scopes[0]);
        var collMethod = cfgs.GetValueOrDefault(scopes[1]);
        var payMethod = cfgs.GetValueOrDefault(scopes[2]);

        var subtype = string.IsNullOrWhiteSpace(cmd.Subtype)
            ? await subtypeRepo.GetByNameAsync("Ninguno", ct)
            : await subtypeRepo.GetByHomologatedCodeAsync(cmd.Subtype, ct);

        var subtypeCfg = subtype is null ? null : await cfgRepo.GetByUuidAsync(subtype.Category, ct);

        var channel = await channelRepo.FindByHomologatedCodeAsync(cmd.Channel, ct);

        return new ContributionCatalogs(
            source, originMod, collMethod, payMethod, channel, subtype, subtypeCfg);
    }
}