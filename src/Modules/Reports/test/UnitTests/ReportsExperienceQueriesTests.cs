using Common.SharedKernel.Application.Reports;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Reports.Application.Reports.Strategies;
using Reports.Domain.TransmissionFormat;
using Reports.Presentation.GraphQL;

namespace Reports.test.UnitTests;

public class ReportsExperienceQueriesTests
{
    [Fact]
    public async Task GetReportDataAsync_ReturnsReportContent()
    {
        var request = new TransmissionFormatReportRequest { GenerationDate = DateTime.UtcNow };
        var expected = new ReportResponseDto { FileContent = "sample", FileName = "file.txt", MimeType = "text/plain" };
        var strategyMock = new Mock<IReportStrategy>();
        var fileResultMock = new Mock<IActionResult>();

        strategyMock
            .Setup(s => s.GetReportDataAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileResultMock.Object);

        var factoryMock = new Mock<IReportStrategyFactory>();
        factoryMock
            .Setup(f => f.GetStrategy(ReportType.TransmissionFormat))
            .Returns(strategyMock.Object);
        
        var queries = new ReportsExperienceQueries(factoryMock.Object);

        var result = await queries.GetReportDataAsync(request, ReportType.TransmissionFormat, CancellationToken.None);

        // Compare with .Object instead of the mock itself
        result.Should().Be(fileResultMock.Object);
    }
}
