using System;
using System.Threading;
using System.Threading.Tasks;
using Common.SharedKernel.Application.Reports;
using FluentAssertions;
using Moq;
using MediatR;
using Reports.Application.Strategies;
using Reports.Domain.DailyClosing;
using Reports.Presentation.GraphQL;
using Xunit;

namespace Reports.test.UnitTests;

public class ReportsExperienceQueriesTests
{
    [Fact]
    public async Task GetReportDataAsync_ReturnsReportContent()
    {
        var request = new DailyClosingReportRequest { PortfolioId = 1, GenerationDate = DateTime.UtcNow };
        var expected = new ReportResponseDto { FileContent = "sample", FileName = "file.txt", MimeType = "text/plain" };
        var strategyMock = new Mock<IReportStrategy>();
        strategyMock
            .Setup(s => s.GetReportDataAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var factoryMock = new Mock<IReportStrategyFactory>();
        factoryMock
            .Setup(f => f.GetStrategy(ReportType.DailyClosing))
            .Returns(strategyMock.Object);
        
        var queries = new ReportsExperienceQueries(factoryMock.Object);

        var result = await queries.GetReportDataAsync(request, ReportType.DailyClosing, CancellationToken.None);

        result.Should().Be(expected);
    }
}
