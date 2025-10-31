using System.Linq;
using System.Text.Json;

using Closing.Application.Abstractions.External.Operations.TrustOperations;
using Closing.Application.PreClosing.Services.ExtraReturns;
using Closing.Application.PreClosing.Services.ExtraReturns.Dto;
using Closing.Application.PreClosing.Services.Yield.Builders;
using Closing.Application.PreClosing.Services.Yield.Constants;
using Closing.Application.PreClosing.Services.Yield.Dto;
using Closing.Integrations.PreClosing.RunSimulation;

using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using FluentAssertions;

using Moq;

namespace Closing.test.UnitTests.Application.PreClosing.Services.ExtraReturns;

public class ExtraReturnConsolidationServiceTests
{
    private readonly Mock<ITrustOperationsLocator> locatorMock = new();

    private ExtraReturnConsolidationService CreateSut() => new(locatorMock.Object);

    [Fact]
    public async Task GetExtraReturnSummariesAsync_ShouldMapLocatorResponse()
    {
        // Arrange
        var portfolioId = 55;
        var closingDate = new DateTime(2024, 8, 15);

        var operations = new List<TrustOperationRemoteResponse>
        {
            new(
                PortfolioId: portfolioId,
                ProcessDateUtc: new DateTime(2024, 8, 14, 12, 0, 0, DateTimeKind.Utc),
                OperationTypeId: 101,
                OperationTypeName: "Rendimientos",
                Amount: 500m),
            new(
                PortfolioId: portfolioId,
                ProcessDateUtc: new DateTime(2024, 8, 14, 13, 0, 0, DateTimeKind.Utc),
                OperationTypeId: 102,
                OperationTypeName: "Rendimientos Ajuste",
                Amount: -200m)
        };

        locatorMock
            .Setup(locator => locator.GetTrustOperationsAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<TrustOperationRemoteResponse>>(operations));

        var sut = CreateSut();

        // Act
        var result = await sut.GetExtraReturnSummariesAsync(portfolioId, closingDate, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        var first = result.Value.First();
        first.PortfolioId.Should().Be(portfolioId);
        first.ProcessDateUtc.Should().Be(new DateTime(2024, 8, 14, 12, 0, 0, DateTimeKind.Utc));
        first.OperationTypeId.Should().Be(101);
        first.OperationTypeName.Should().Be("Rendimientos");
        first.Amount.Should().Be(-500m);
        first.Concept.Should().Be(JsonSerializer.Serialize(new StringEntityDto("101", "Rendimientos")));

        var second = result.Value.ElementAt(1);
        second.Amount.Should().Be(200m);
        second.Concept.Should().Be(JsonSerializer.Serialize(new StringEntityDto("102", "Rendimientos Ajuste")));

        locatorMock.Verify(locator => locator.GetTrustOperationsAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetExtraReturnSummariesAsync_ShouldPropagateLocatorFailure()
    {
        // Arrange
        var error = Error.Validation("OPS_FAIL", "Falló la consulta");

        locatorMock
            .Setup(locator => locator.GetTrustOperationsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<IReadOnlyCollection<TrustOperationRemoteResponse>>(error));

        var sut = CreateSut();

        // Act
        var result = await sut.GetExtraReturnSummariesAsync(10, DateTime.UtcNow, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(error.Code);
        result.Error.Description.Should().Be(error.Description);
        result.Error.Type.Should().Be(error.Type);
    }

    [Fact]
    public async Task GetExtraReturnSummariesAsync_ShouldFailWhenLocatorReturnsEmpty()
    {
        // Arrange
        locatorMock
            .Setup(locator => locator.GetTrustOperationsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<TrustOperationRemoteResponse>>(Array.Empty<TrustOperationRemoteResponse>()));

        var sut = CreateSut();

        // Act
        var result = await sut.GetExtraReturnSummariesAsync(10, DateTime.UtcNow, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ExtraReturnErrors.NoOperationsFound.Code);
        result.Error.Description.Should().Be(ExtraReturnErrors.NoOperationsFound.Description);
        result.Error.Type.Should().Be(ExtraReturnErrors.NoOperationsFound.Type);
    }

    [Fact]
    public async Task GetExtraReturnSummariesAsync_ShouldProduceSummariesConsistentWithYieldDetailRequirements()
    {
        // Arrange
        var portfolioId = 25;
        var closingDate = new DateTime(2024, 9, 30);
        var processDateUtc = new DateTime(2024, 9, 30, 12, 0, 0, DateTimeKind.Utc);
        var operationTypeId = 3050L;
        var operationTypeName = "Ajuste Rendimientos";
        var amount = 9876.45m;

        var operations = new List<TrustOperationRemoteResponse>
        {
            new(
                PortfolioId: portfolioId,
                ProcessDateUtc: processDateUtc,
                OperationTypeId: operationTypeId,
                OperationTypeName: operationTypeName,
                Amount: amount)
        };

        locatorMock
            .Setup(locator => locator.GetTrustOperationsAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<TrustOperationRemoteResponse>>(operations));

        var sut = CreateSut();

        // Act
        var result = await sut.GetExtraReturnSummariesAsync(portfolioId, closingDate, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var summary = result.Value.Should().ContainSingle().Which;

        summary.PortfolioId.Should().Be(portfolioId);
        summary.ProcessDateUtc.Should().Be(processDateUtc);
        summary.OperationTypeId.Should().Be(operationTypeId);
        summary.OperationTypeName.Should().Be(operationTypeName);
        summary.Amount.Should().Be(-amount);

        using var summaryConcept = JsonDocument.Parse(summary.Concept);
        summaryConcept.RootElement.GetProperty("EntityId").GetString().Should().Be(operationTypeId.ToString());
        summaryConcept.RootElement.GetProperty("EntityValue").GetString().Should().Be(operationTypeName);

        var builder = new ExtraReturnYieldDetailBuilder();
        var parameters = new RunSimulationParameters(portfolioId, closingDate, IsClosing: true);
        var yieldDetail = builder.Build(summary, parameters);

        yieldDetail.PortfolioId.Should().Be(portfolioId);
        yieldDetail.ClosingDate.Should().Be(DateTime.SpecifyKind(closingDate, DateTimeKind.Utc));
        yieldDetail.Source.Should().Be(YieldsSources.ExtraReturn);
        yieldDetail.Income.Should().Be(amount);
        yieldDetail.Expenses.Should().Be(0m);
        yieldDetail.Commissions.Should().Be(0m);
        yieldDetail.ProcessDate.Should().Be(processDateUtc);
        yieldDetail.IsClosed.Should().BeTrue();
        yieldDetail.Concept.RootElement.GetProperty("EntityId").GetString().Should().Be(operationTypeId.ToString());
        yieldDetail.Concept.RootElement.GetProperty("EntityValue").GetString().Should().Be(operationTypeName);

        locatorMock.Verify(locator => locator.GetTrustOperationsAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()), Times.Once);
    }
}
