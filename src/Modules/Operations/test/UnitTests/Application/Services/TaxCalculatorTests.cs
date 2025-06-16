using System.Text.Json;
using Common.SharedKernel.Domain.ConfigurationParameters;
using FluentAssertions;
using Moq;
using Operations.Application.Contributions.CreateContribution;
using Operations.Application.Contributions.Services;
using Operations.Domain.ConfigurationParameters;

namespace Operations.test.UnitTests.Application.Services;

public class TaxCalculatorTests
{
    private readonly Mock<IConfigurationParameterRepository> _repo = new();

    private static ConfigurationParameter Param(string name, string code, string metadata = "\"0%\"")
    {
        return ConfigurationParameter.Create(name, code, metadata: JsonDocument.Parse(metadata));
    }

    [Fact]
    public async Task ComputeAsync_Should_Return_Zero_With_Pensioner()
    {
        var tax = Param("Exento", "E");
        var uncert = Param("Uncert", "U");
        _repo.Setup(r => r.GetByUuidsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ConfigurationParameter>
            {
                { ConfigurationParameterUuids.TaxExempt, tax },
                { ConfigurationParameterUuids.UncertifiedState, uncert }
            });
        var calc = new TaxCalculator(_repo.Object);

        var result = await calc.ComputeAsync(true, false, 500m, CancellationToken.None);

        result.WithheldAmount.Should().Be(0m);
        result.TaxConditionName.Should().Be("Exento");
    }

    [Fact]
    public async Task ComputeAsync_Should_Not_Withhold_When_Certified()
    {
        var tax = Param("SinRet", "NR");
        var cert = Param("Cert", "C");
        _repo.Setup(r => r.GetByUuidsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ConfigurationParameter>
            {
                { ConfigurationParameterUuids.TaxNoRetention, tax },
                { ConfigurationParameterUuids.CertifiedState, cert }
            });
        var calc = new TaxCalculator(_repo.Object);

        var result = await calc.ComputeAsync(false, true, 1000m, CancellationToken.None);

        result.WithheldAmount.Should().Be(0m);
        result.TaxConditionName.Should().Be("SinRet");
    }

    [Theory]
    [InlineData("\"10%\"", 1000, 100)]
    [InlineData("\"bad\"", 1000, 0)]
    [InlineData(@"{""valor"":""7%""}", 1000, 70)]
    [InlineData(@"{""valor"":""7,5%""}", 2000, 150)]
    [InlineData("7.5", 1000, 75)]
    public async Task ComputeAsync_Should_Withhold_With_New_Formats(string meta, decimal amount, decimal expected)
    {
        var tax = Param("Retencion", "R");
        var uncert = Param("Uncert", "U");
        var pct = Param("Pct", "P", meta);

        _repo.Setup(r => r.GetByUuidsAsync(It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ConfigurationParameter>
            {
                { ConfigurationParameterUuids.TaxRetention, tax },
                { ConfigurationParameterUuids.UncertifiedState, uncert },
                { ConfigurationParameterUuids.RetentionPct, pct }
            });

        var calc = new TaxCalculator(_repo.Object);
        var result = await calc.ComputeAsync(false, false, amount, CancellationToken.None);

        result.WithheldAmount.Should().Be(expected);
        result.TaxConditionName.Should().Be("Retencion");
    }

    [Theory]
    [InlineData("\"10%\"", 1000, 100)]
    [InlineData("\"bad\"", 1000, 0)]
    public async Task ComputeAsync_Should_Withhold_When_Retention(string meta, decimal amount, decimal expected)
    {
        var tax = Param("Retencion", "R");
        var uncert = Param("Uncert", "U");
        var pct = Param("Pct", "P", meta);
        _repo.Setup(r => r.GetByUuidsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ConfigurationParameter>
            {
                { ConfigurationParameterUuids.TaxRetention, tax },
                { ConfigurationParameterUuids.UncertifiedState, uncert },
                { ConfigurationParameterUuids.RetentionPct, pct }
            });
        var calc = new TaxCalculator(_repo.Object);

        var result = await calc.ComputeAsync(false, false, amount, CancellationToken.None);

        result.WithheldAmount.Should().Be(expected);
        result.TaxConditionName.Should().Be("Retencion");
    }

    [Theory]
    [InlineData("\"10%\"", 0.10)]
    [InlineData(@"{""valor"":""7%""}", 0.07)]
    [InlineData(@"{""valor"":""7,5%""}", 0.075)]
    [InlineData("7.5", 0.075)]
    [InlineData("7", 0.07)]
    [InlineData("\"bad\"", 0.00)]
    public void ExtractPercent_Should_Parse_All_Supported_Formats(string meta, decimal expected)
    {
        var doc = JsonDocument.Parse(meta);
        TaxCalculator.ExtractPercent(doc).Should().Be(expected);
    }

    [Theory]
    [InlineData("10%", 0.10)]
    [InlineData("7,5%", 0.075)]
    [InlineData("7.5", 0.075)]
    [InlineData("7", 0.07)]
    [InlineData("bad", 0.00)]
    [InlineData("", 0.00)]
    public void ParseFraction_Should_Work_With_Various_Strings(string raw, decimal expected)
    {
        TaxCalculator.ParseFraction(raw).Should().Be(expected);
    }
}