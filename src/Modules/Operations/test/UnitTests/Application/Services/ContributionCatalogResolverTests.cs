using Operations.Domain.ConfigurationParameters;
using System.Text.Json;
using FluentAssertions;
using Moq;
using Operations.Application.Abstractions.Data;
using Operations.Application.Contributions.Services;
using Operations.Domain.Channels;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Domain;
using Operations.Domain.Origins;
using Operations.Domain.SubtransactionTypes;
using Operations.Integrations.Contributions.CreateContribution;

namespace Operations.test.UnitTests.Application.Services;

public class ContributionCatalogResolverTests
{
    private readonly Mock<IOriginRepository> _originRepo = new();
    private readonly Mock<IConfigurationParameterRepository> _cfgRepo = new();
    private readonly Mock<ISubtransactionTypeRepository> _subRepo = new();
    private readonly Mock<IChannelRepository> _chanRepo = new();

    private static Origin BuildOrigin() => Origin.Create("o", false, false, false, Status.Active, "SRC").Value;
    private static ConfigurationParameter Cfg(string name, string code) => ConfigurationParameter.Create(name, code);
    private static SubtransactionType Subtype() => SubtransactionType.Create("s", Guid.NewGuid(), "N", Status.Active, "EXT", "SUB").Value;
    private static Channel BuildChannel() => Channel.Create("c", "CH", false, Status.Active).Value;

    private static CreateContributionCommand Cmd(string? subtype = "SUB")
    {
        return new CreateContributionCommand(
            "CC",
            "123",
            1,
            "1",
            100m,
            "SRC",
            "MOD",
            "COL",
            "PAY",
            JsonDocument.Parse("{}"),
            "BANK",
            "ACC",
            "SI",
            null,
            DateTime.Today,
            DateTime.Today,
            "s",
            JsonDocument.Parse("{}"),
            subtype,
            "WEB",
            "u");
    }

    private ContributionCatalogResolver BuildResolver()
    {
        return new ContributionCatalogResolver(_originRepo.Object, _cfgRepo.Object, _subRepo.Object, _chanRepo.Object);
    }

    [Fact]
    public async Task ResolveAsync_Should_Return_Catalogs_With_Subtype()
    {
        var origin = BuildOrigin();
        var mod = Cfg("mod", "MOD");
        var coll = Cfg("coll", "COL");
        var pay = Cfg("pay", "PAY");
        var sub = Subtype();
        var subCfg = Cfg("cat", "CAT");
        var chan = BuildChannel();

        _originRepo.Setup(r => r.FindByHomologatedCodeAsync("SRC", It.IsAny<CancellationToken>()))
            .ReturnsAsync(origin);
        _cfgRepo.Setup(r => r.GetByCodesAndTypesAsync(It.IsAny<IEnumerable<(string Code, string Scope)>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<(string, string), ConfigurationParameter>
            {
                { ("MOD", "Modalidad Origen Aporte"), mod },
                { ("COL", "Metodo de Recaudo"), coll },
                { ("PAY", "Metodo de Pago"), pay }
            });
        _subRepo.Setup(r => r.GetByHomologatedCodeAsync("SUB", It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);
        _cfgRepo.Setup(r => r.GetByUuidAsync(sub.Category, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subCfg);
        _chanRepo.Setup(r => r.FindByHomologatedCodeAsync("WEB", It.IsAny<CancellationToken>()))
            .ReturnsAsync(chan);
        var resolver = BuildResolver();
        var cmd = Cmd();

        var result = await resolver.ResolveAsync(cmd, CancellationToken.None);

        result.Source.Should().Be(origin);
        result.OriginModality.Should().Be(mod);
        result.CollectionMethod.Should().Be(coll);
        result.PaymentMethod.Should().Be(pay);
        result.Subtype.Should().Be(sub);
        result.SubtypeCategoryCfg.Should().Be(subCfg);
        result.Channel.Should().Be(chan);
    }

    [Fact]
    public async Task ResolveAsync_Should_Use_Default_Subtype_When_Missing()
    {
        var sub = Subtype();
        _subRepo.Setup(r => r.GetByNameAsync("Ninguno", It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);
        _cfgRepo.Setup(r => r.GetByCodesAndTypesAsync(It.IsAny<IEnumerable<(string,string)>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<(string,string), ConfigurationParameter>());
        var resolver = BuildResolver();
        var cmd = Cmd(null);

        var result = await resolver.ResolveAsync(cmd, CancellationToken.None);

        result.Subtype.Should().Be(sub);
    }

    [Fact]
    public async Task ResolveAsync_Should_Handle_Null_Subtype()
    {
        _subRepo.Setup(r => r.GetByNameAsync("Ninguno", It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubtransactionType?)null);
        _cfgRepo.Setup(r => r.GetByCodesAndTypesAsync(It.IsAny<IEnumerable<(string,string)>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<(string,string), ConfigurationParameter>());
        var resolver = BuildResolver();
        var cmd = Cmd(string.Empty);

        var result = await resolver.ResolveAsync(cmd, CancellationToken.None);

        result.Subtype.Should().BeNull();
        result.SubtypeCategoryCfg.Should().BeNull();
        _cfgRepo.Verify(r => r.GetByUuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}