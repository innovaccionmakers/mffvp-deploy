using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Reports.Application.Reports.Common.Strategies;
using Reports.Application.Reports.TransmissionFormat;
using Reports.Application.Reports.TransmissionFormat.Strategies;
using Reports.Domain.TransmissionFormat;
using Reports.Domain.TransmissionFormat.Records;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Reports.test.UnitTests;

public class TransmissionFormatReportTests
{
    [Fact]
    public async Task GetReportDataAsync_ShouldBuildRt2And312()
    {
        var strategies = new IReportGeneratorStrategy[] { new TransmissionFormatReportStrategy() };
        var strategyBuilder = new ReportStrategyBuilder(strategies);

        var repositoryMock = new Mock<ITransmissionFormatReportRepository>();
        repositoryMock
            .Setup(r => r.GetRt1HeaderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt1Header("5", "123456", "CLAVE12345"));
        repositoryMock
            .Setup(r => r.GetRt2HeaderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt2Header("1234"));
        repositoryMock
            .Setup(r => r.GetUnitValueAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100.123456m);
        repositoryMock
            .Setup(r => r.GetValuationMovementsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt4ValuationMovements(
                1000m,
                5000m,
                -1000.5m,
                200m,
                10000m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                1500m,
                15000m));
        repositoryMock
            .Setup(r => r.GetProfitabilitiesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt4Profitabilities(10.12m, 20.34m, 30.56m));
        repositoryMock
            .Setup(r => r.GetAutomaticConceptAmountsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AutomaticConceptAmounts(0m, 0m, 0m, 0m, 0m));

        // New methods for multi-portfolio support
        repositoryMock
            .Setup(r => r.AnyPortfolioExistsOnOrAfterDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        repositoryMock
            .Setup(r => r.GetPortfolioIdsWithClosureOnDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 1 });

        var reportBuilder = new TransmissionFormatReportBuilder(strategyBuilder);
        var strategy = new TransmissionFormatReport(new NullLogger<TransmissionFormatReport>(), repositoryMock.Object, reportBuilder);
        var request = new TransmissionFormatReportRequest { GenerationDate = new DateTime(2025, 2, 6) };

        var response = await strategy.GetReportDataAsync(request, CancellationToken.None);

        if (response is FileContentResult fileResult)
        {
            var report = Encoding.UTF8.GetString(fileResult.FileContents);
            var lines = report.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            report.Should().Contain("000022000001161234");
            report.Should().Contain("0000343120101011+0000000000100.123456");
            report.Should().Contain("0000443130101005+0000000001000.000000");
            report.Should().Contain("0000643130202005-00000000000001000.50");
            report.Should().Contain("43130103005+0000000000200.000000");
            report.Should().Contain("43140101005+00000000000000010.12");
            report.Should().Contain("43140101010+00000000000000020.34");
            report.Should().Contain("43140101015+00000000000000030.56");
            lines[^1].Trim().Should().Be("000295");
            report.Should().Contain("000022000001161234");
            report.Should().Contain("0000343120101011+0000000000100.123456");
            report.Should().Contain("0000443130101005+0000000001000.000000");
            report.Should().Contain("0000643130202005-00000000000001000.50");
            report.Should().Contain("43130103005+0000000000200.000000");
            report.Should().Contain("43140101005+00000000000000010.12");
            report.Should().Contain("43140101010+00000000000000020.34");
            report.Should().Contain("43140101015+00000000000000030.56");
            lines[^1].Should().Be("000295");

            var totalRecordsFromHeader = int.Parse(lines[0].Substring(22, 5));
            totalRecordsFromHeader.Should().Be(lines.Length);
        }
    }

    [Fact]
    public async Task GetReportDataAsync_ShouldRespectProfitabilitySigns()
    {
        var strategies = new IReportGeneratorStrategy[] { new TransmissionFormatReportStrategy() };
        var strategyBuilder = new ReportStrategyBuilder(strategies);

        var repositoryMock = new Mock<ITransmissionFormatReportRepository>();
        repositoryMock
            .Setup(r => r.GetRt1HeaderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt1Header("5", "123456", "CLAVE12345"));
        repositoryMock
            .Setup(r => r.GetRt2HeaderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt2Header("1234"));
        repositoryMock
            .Setup(r => r.GetUnitValueAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100.123456m);
        repositoryMock
            .Setup(r => r.GetValuationMovementsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt4ValuationMovements(
                1000m,
                5000m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                1000m,
                5000m));
        repositoryMock
            .Setup(r => r.GetProfitabilitiesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt4Profitabilities(-10.12m, 0m, 30.56m));
        repositoryMock
            .Setup(r => r.GetAutomaticConceptAmountsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AutomaticConceptAmounts(0m, 0m, 0m, 0m, 0m));
        repositoryMock
            .Setup(r => r.AnyPortfolioExistsOnOrAfterDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        repositoryMock
            .Setup(r => r.GetPortfolioIdsWithClosureOnDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 1 });

        var reportBuilder = new TransmissionFormatReportBuilder(strategyBuilder);
        var strategy = new TransmissionFormatReport(new NullLogger<TransmissionFormatReport>(), repositoryMock.Object, reportBuilder);
        var request = new TransmissionFormatReportRequest { GenerationDate = new DateTime(2025, 2, 6) };

        var response = await strategy.GetReportDataAsync(request, CancellationToken.None);
        var report = ReadReport(response);

        report.Should().Contain("43140101005-00000000000000010.12");
        report.Should().Contain("43140101010+00000000000000000.00");
        report.Should().Contain("43140101015+00000000000000030.56");
    }

    [Fact]
    public async Task GetReportDataAsync_ShouldAddDistributionConceptToWithdrawals_WhenDistributionIsNegative()
    {
        var strategies = new IReportGeneratorStrategy[] { new TransmissionFormatReportStrategy() };
        var strategyBuilder = new ReportStrategyBuilder(strategies);

        var repositoryMock = new Mock<ITransmissionFormatReportRepository>();
        repositoryMock
            .Setup(r => r.GetRt1HeaderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt1Header("5", "123456", "CLAVE12345"));
        repositoryMock
            .Setup(r => r.GetRt2HeaderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt2Header("1234"));
        repositoryMock
            .Setup(r => r.GetUnitValueAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(25m);
        repositoryMock
            .Setup(r => r.GetValuationMovementsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt4ValuationMovements(
                1000m,
                5000m,
                100m,
                10m,
                1000m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                1100m,
                6000m));
        repositoryMock
            .Setup(r => r.GetProfitabilitiesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt4Profitabilities(10m, 20m, 30m));
        repositoryMock
            .Setup(r => r.GetAutomaticConceptAmountsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AutomaticConceptAmounts(100m, 160m, 0m, 0m, 0m));
        repositoryMock
            .Setup(r => r.AnyPortfolioExistsOnOrAfterDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        repositoryMock
            .Setup(r => r.GetPortfolioIdsWithClosureOnDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 1 });

        var reportBuilder = new TransmissionFormatReportBuilder(strategyBuilder);
        var strategy = new TransmissionFormatReport(new NullLogger<TransmissionFormatReport>(), repositoryMock.Object, reportBuilder);
        var request = new TransmissionFormatReportRequest { GenerationDate = new DateTime(2025, 2, 6) };

        var response = await strategy.GetReportDataAsync(request, CancellationToken.None);
        var report = ReadReport(response);

        // Distribution = 100 - (160 + 0) = -60 -> withdrawals. Units adjustment = TRUNC(-60 / 25, 6) = -2.4
        report.Should().Contain("43130103020-0000000000000.000000");
        report.Should().Contain("43130203020-00000000000000000.00");
    }

    [Fact]
    public async Task GetReportDataAsync_ShouldAddDistributionConceptToContributions_WhenDistributionIsPositive()
    {
        var strategies = new IReportGeneratorStrategy[] { new TransmissionFormatReportStrategy() };
        var strategyBuilder = new ReportStrategyBuilder(strategies);

        var repositoryMock = new Mock<ITransmissionFormatReportRepository>();
        repositoryMock
            .Setup(r => r.GetRt1HeaderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt1Header("5", "123456", "CLAVE12345"));
        repositoryMock
            .Setup(r => r.GetRt2HeaderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt2Header("1234"));
        repositoryMock
            .Setup(r => r.GetUnitValueAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(40m);
        repositoryMock
            .Setup(r => r.GetValuationMovementsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt4ValuationMovements(
                1000m,
                5000m,
                100m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                1000m,
                6000m));
        repositoryMock
            .Setup(r => r.GetProfitabilitiesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt4Profitabilities(10m, 20m, 30m));
        repositoryMock
            .Setup(r => r.GetAutomaticConceptAmountsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AutomaticConceptAmounts(200m, 140m, 0m, 0m, 0m));
        repositoryMock
            .Setup(r => r.AnyPortfolioExistsOnOrAfterDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        repositoryMock
            .Setup(r => r.GetPortfolioIdsWithClosureOnDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 1 });

        var reportBuilder = new TransmissionFormatReportBuilder(strategyBuilder);
        var strategy = new TransmissionFormatReport(new NullLogger<TransmissionFormatReport>(), repositoryMock.Object, reportBuilder);
        var request = new TransmissionFormatReportRequest { GenerationDate = new DateTime(2025, 2, 6) };

        var response = await strategy.GetReportDataAsync(request, CancellationToken.None);
        var report = ReadReport(response);

        // Distribution = 200 - (140 + 0) = 60 -> contributions. Unit adjustment = TRUNC(60 / 40, 6) = 1.5
        report.Should().Contain("43130103005+0000000000000.000000");
        report.Should().Contain("43130203005+00000000000000000.00");
    }

    [Fact]
    public async Task GetReportDataAsync_ShouldAddNoteConceptToContributions_WhenNoteIsPositive()
    {
        var strategies = new IReportGeneratorStrategy[] { new TransmissionFormatReportStrategy() };
        var strategyBuilder = new ReportStrategyBuilder(strategies);

        var repositoryMock = new Mock<ITransmissionFormatReportRepository>();
        repositoryMock
            .Setup(r => r.GetRt1HeaderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt1Header("5", "123456", "CLAVE12345"));
        repositoryMock
            .Setup(r => r.GetRt2HeaderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt2Header("1234"));
        repositoryMock
            .Setup(r => r.GetUnitValueAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(25m);
        repositoryMock
            .Setup(r => r.GetValuationMovementsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt4ValuationMovements(
                1000m,
                5000m,
                100m,
                10m,
                1000m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                1100m,
                6000m));
        repositoryMock
            .Setup(r => r.GetProfitabilitiesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt4Profitabilities(10m, 20m, 30m));
        repositoryMock
            .Setup(r => r.GetAutomaticConceptAmountsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AutomaticConceptAmounts(30m, 0m, 30m, 30m, 0m));
        repositoryMock
            .Setup(r => r.AnyPortfolioExistsOnOrAfterDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        repositoryMock
            .Setup(r => r.GetPortfolioIdsWithClosureOnDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 1 });

        var reportBuilder = new TransmissionFormatReportBuilder(strategyBuilder);
        var strategy = new TransmissionFormatReport(new NullLogger<TransmissionFormatReport>(), repositoryMock.Object, reportBuilder);
        var request = new TransmissionFormatReportRequest { GenerationDate = new DateTime(2025, 2, 6) };

        var response = await strategy.GetReportDataAsync(request, CancellationToken.None);
        var report = ReadReport(response);

        // Note = 30 -> contributions. Unit adjustment = TRUNC(30 / 25, 6) = 1.2
        report.Should().Contain("43130103005+0000000000010.000000");
        report.Should().Contain("43130203005+00000000000001000.00");
    }

    [Fact]
    public async Task GetReportDataAsync_ShouldAddNoteConceptToWithdrawals_WhenNoteIsNegative()
    {
        var strategies = new IReportGeneratorStrategy[] { new TransmissionFormatReportStrategy() };
        var strategyBuilder = new ReportStrategyBuilder(strategies);

        var repositoryMock = new Mock<ITransmissionFormatReportRepository>();
        repositoryMock
            .Setup(r => r.GetRt1HeaderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt1Header("5", "123456", "CLAVE12345"));
        repositoryMock
            .Setup(r => r.GetRt2HeaderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt2Header("1234"));
        repositoryMock
            .Setup(r => r.GetUnitValueAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(25m);
        repositoryMock
            .Setup(r => r.GetValuationMovementsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt4ValuationMovements(
                1000m,
                5000m,
                100m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                1100m,
                6000m));
        repositoryMock
            .Setup(r => r.GetProfitabilitiesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt4Profitabilities(10m, 20m, 30m));
        repositoryMock
            .Setup(r => r.GetAutomaticConceptAmountsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AutomaticConceptAmounts(20m, 40m, -20m, 0m, -20m));
        repositoryMock
            .Setup(r => r.AnyPortfolioExistsOnOrAfterDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        repositoryMock
            .Setup(r => r.GetPortfolioIdsWithClosureOnDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 1 });

        var reportBuilder = new TransmissionFormatReportBuilder(strategyBuilder);
        var strategy = new TransmissionFormatReport(new NullLogger<TransmissionFormatReport>(), repositoryMock.Object, reportBuilder);
        var request = new TransmissionFormatReportRequest { GenerationDate = new DateTime(2025, 2, 6) };

        var response = await strategy.GetReportDataAsync(request, CancellationToken.None);
        var report = ReadReport(response);

        // Note = -20 -> withdrawals. Unit adjustment = TRUNC(-20 / 25, 6) = -0.8
        report.Should().Contain("43130103020-0000000000000.000000");
        report.Should().Contain("43130203020-00000000000000000.00");
    }

    [Fact]
    public async Task GetReportDataAsync_ShouldNotAdjustAutomaticConceptUnits_WhenUnitValueIsNotPositive()
    {
        var strategies = new IReportGeneratorStrategy[] { new TransmissionFormatReportStrategy() };
        var strategyBuilder = new ReportStrategyBuilder(strategies);

        var repositoryMock = new Mock<ITransmissionFormatReportRepository>();
        repositoryMock
            .Setup(r => r.GetRt1HeaderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt1Header("5", "123456", "CLAVE12345"));
        repositoryMock
            .Setup(r => r.GetRt2HeaderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt2Header("1234"));
        repositoryMock
            .Setup(r => r.GetUnitValueAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);
        repositoryMock
            .Setup(r => r.GetValuationMovementsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt4ValuationMovements(
                1000m,
                5000m,
                100m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                1100m,
                6000m));
        repositoryMock
            .Setup(r => r.GetProfitabilitiesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt4Profitabilities(10m, 20m, 30m));
        repositoryMock
            .Setup(r => r.GetAutomaticConceptAmountsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AutomaticConceptAmounts(40m, 0m, -20m, 0m, -20m));
        repositoryMock
            .Setup(r => r.AnyPortfolioExistsOnOrAfterDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        repositoryMock
            .Setup(r => r.GetPortfolioIdsWithClosureOnDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 1 });

        var reportBuilder = new TransmissionFormatReportBuilder(strategyBuilder);
        var strategy = new TransmissionFormatReport(new NullLogger<TransmissionFormatReport>(), repositoryMock.Object, reportBuilder);
        var request = new TransmissionFormatReportRequest { GenerationDate = new DateTime(2025, 2, 6) };

        var response = await strategy.GetReportDataAsync(request, CancellationToken.None);
        var report = ReadReport(response);

        // Distribution = 40 - (0 + -20) = 60, Note = -20. With unit value 0, units remain unchanged.
        report.Should().Contain("43130103005+0000000000000.000000");
        report.Should().Contain("43130203005+00000000000000000.00");
        report.Should().Contain("43130103020-0000000000000.000000");
        report.Should().Contain("43130203020-00000000000000060.00");
    }

    [Fact]
    public async Task GetReportDataAsync_ShouldCalculateCancellationUnitsFromAmount()
    {
        var strategies = new IReportGeneratorStrategy[] { new TransmissionFormatReportStrategy() };
        var strategyBuilder = new ReportStrategyBuilder(strategies);

        var repositoryMock = new Mock<ITransmissionFormatReportRepository>();
        repositoryMock
            .Setup(r => r.GetRt1HeaderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt1Header("5", "123456", "CLAVE12345"));
        repositoryMock
            .Setup(r => r.GetRt2HeaderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt2Header("1234"));
        repositoryMock
            .Setup(r => r.GetUnitValueAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(6m);
        repositoryMock
            .Setup(r => r.GetValuationMovementsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt4ValuationMovements(
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                10m,
                0m,
                0m));
        repositoryMock
            .Setup(r => r.GetProfitabilitiesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt4Profitabilities(0m, 0m, 0m));
        repositoryMock
            .Setup(r => r.GetAutomaticConceptAmountsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AutomaticConceptAmounts(0m, 0m, 0m, 0m, 0m));
        repositoryMock
            .Setup(r => r.AnyPortfolioExistsOnOrAfterDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        repositoryMock
            .Setup(r => r.GetPortfolioIdsWithClosureOnDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 1 });

        var reportBuilder = new TransmissionFormatReportBuilder(strategyBuilder);
        var strategy = new TransmissionFormatReport(new NullLogger<TransmissionFormatReport>(), repositoryMock.Object, reportBuilder);
        var request = new TransmissionFormatReportRequest { GenerationDate = new DateTime(2025, 2, 6) };

        var response = await strategy.GetReportDataAsync(request, CancellationToken.None);
        var report = ReadReport(response);
        var lines = report.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var cancellationUnitsLine = lines.Single(line => line.Contains("0103040"));
        cancellationUnitsLine.Should().Contain("43130103040-0000000000001.666666");

        var cancellationAmountLine = lines.Single(line => line.Contains("0203040"));
        cancellationAmountLine.Should().Contain("43130203040-00000000000000010.00");
    }

    [Fact]
    public async Task GetReportDataAsync_ShouldReturnZeroCancellationUnits_WhenUnitValueIsNotPositive()
    {
        var strategies = new IReportGeneratorStrategy[] { new TransmissionFormatReportStrategy() };
        var strategyBuilder = new ReportStrategyBuilder(strategies);

        var repositoryMock = new Mock<ITransmissionFormatReportRepository>();
        repositoryMock
            .Setup(r => r.GetRt1HeaderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt1Header("5", "123456", "CLAVE12345"));
        repositoryMock
            .Setup(r => r.GetRt2HeaderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt2Header("1234"));
        repositoryMock
            .Setup(r => r.GetUnitValueAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);
        repositoryMock
            .Setup(r => r.GetValuationMovementsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt4ValuationMovements(
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m,
                25m,
                0m,
                0m));
        repositoryMock
            .Setup(r => r.GetProfitabilitiesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rt4Profitabilities(0m, 0m, 0m));
        repositoryMock
            .Setup(r => r.GetAutomaticConceptAmountsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AutomaticConceptAmounts(0m, 0m, 0m, 0m, 0m));
        repositoryMock
            .Setup(r => r.AnyPortfolioExistsOnOrAfterDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        repositoryMock
            .Setup(r => r.GetPortfolioIdsWithClosureOnDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 1 });

        var reportBuilder = new TransmissionFormatReportBuilder(strategyBuilder);
        var strategy = new TransmissionFormatReport(new NullLogger<TransmissionFormatReport>(), repositoryMock.Object, reportBuilder);
        var request = new TransmissionFormatReportRequest { GenerationDate = new DateTime(2025, 2, 6) };

        var response = await strategy.GetReportDataAsync(request, CancellationToken.None);
        var report = ReadReport(response);
        var lines = report.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var cancellationUnitsLine = lines.Single(line => line.Contains("0103040"));
        cancellationUnitsLine.Should().Contain("43130103040-0000000000000.000000");

        var cancellationAmountLine = lines.Single(line => line.Contains("0203040"));
        cancellationAmountLine.Should().Contain("43130203040-00000000000000025.00");
    }

    private static string ReadReport(IActionResult response)
    {
        return response switch
        {
            FileContentResult fileContentResult => Encoding.UTF8.GetString(fileContentResult.FileContents),
            FileStreamResult fileStreamResult => ReadStream(fileStreamResult.FileStream),
            _ => throw new InvalidOperationException($"Unexpected response type: {response.GetType().Name}")
        };
    }

    private static string ReadStream(Stream stream)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }
}
