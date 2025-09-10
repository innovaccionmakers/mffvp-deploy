using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Reports.Application.Reports.Common.Strategies;
using Reports.Application.Reports.DailyClosing;
using Reports.Application.Reports.DailyClosing.Strategies;
using Reports.Domain.DailyClosing;
using Reports.Domain.DailyClosing.Records;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Reports.test.UnitTests;

public class DailyClosingReportTests
{
    [Fact]
    public async Task GetReportDataAsync_ShouldBuildRt2And312()
    {
        var strategies = new IReportGeneratorStrategy[] { new DailyClosingReportStrategy() };
        var strategyBuilder = new ReportStrategyBuilder(strategies);

        var repositoryMock = new Mock<IDailyClosingReportRepository>();
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

        var reportBuilder = new DailyClosingReportBuilder(strategyBuilder);
        var strategy = new DailyClosingReport(new NullLogger<DailyClosingReport>(), repositoryMock.Object, reportBuilder);
        var request = new DailyClosingReportRequest { PortfolioId = 1, GenerationDate = new DateTime(2025, 2, 6) };

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
}
