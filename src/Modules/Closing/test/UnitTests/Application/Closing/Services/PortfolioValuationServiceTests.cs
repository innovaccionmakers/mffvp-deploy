

using Closing.Application.Abstractions.External.Operations.OperationTypes;
using Closing.Application.Closing.Services.OperationTypes;
using Closing.Application.Closing.Services.PortfolioValuation;
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Domain.ClientOperations;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.Yields;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.OperationTypes;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Operations.Integrations.OperationTypes;
using System.Text.Json;


namespace Closing.test.UnitTests.Application.Closing.Services;

public class PortfolioValuationServiceTests
{
    private readonly Mock<IPortfolioValuationRepository> portfolioValuationRepositoryMock = new();
    private readonly Mock<IClientOperationRepository> clientOperationRepositoryMock = new();
    private readonly Mock<IYieldRepository> yieldRepositoryMock = new();
    private readonly Mock<IOperationTypesService> operationTypesServiceMock = new();
    private readonly Mock<IConfigurationParameterRepository> configurationParameterRepositoryMock = new();
    private readonly Mock<ITimeControlService> timeControlServiceMock = new();
    private readonly Mock<IOperationTypesLocator> operationTypesLocatorMock = new();
    private readonly Mock<ILogger<PortfolioValuationService>> loggerMock = new();

    private PortfolioValuationService CreateService()
        => new(
            portfolioValuationRepositoryMock.Object,
            clientOperationRepositoryMock.Object,
            yieldRepositoryMock.Object,
            operationTypesServiceMock.Object,
            configurationParameterRepositoryMock.Object,
            timeControlServiceMock.Object,
            loggerMock.Object,
            operationTypesLocatorMock.Object);

    public PortfolioValuationServiceTests()
    {
        timeControlServiceMock
            .Setup(s => s.UpdateStepAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task CalculateAndPersistValuationAsyncReturnsFailureWhenValuationAlreadyExists()
    {
        // Arrange
        var portfolioId = 10;
        var closingDateUtc = new DateTime(2024, 1, 2);
        var cancellationToken = CancellationToken.None;

        portfolioValuationRepositoryMock
            .Setup(r => r.ExistsByPortfolioAndDateAsync(
                portfolioId,
                closingDateUtc.Date,
                cancellationToken))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        var result = await service.CalculateAndPersistValuationAsync(
            portfolioId,
            closingDateUtc,
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("001");
        result.Error!.Type.Should().Be(ErrorType.Validation);

        portfolioValuationRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Domain.PortfolioValuations.PortfolioValuation>(), new CancellationToken()),
            Times.Never);
    }

    [Fact]
    public async Task CalculateAndPersistValuationAsyncReturnsFailureWhenOperationTypesServiceFails()
    {
        // Arrange
        var portfolioId = 20;
        var closingDateUtc = new DateTime(2024, 3, 5);
        var cancellationToken = CancellationToken.None;

        portfolioValuationRepositoryMock
            .Setup(r => r.ExistsByPortfolioAndDateAsync(
                portfolioId,
                closingDateUtc.Date,
                cancellationToken))
            .ReturnsAsync(false);

        portfolioValuationRepositoryMock
            .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(
                portfolioId,
                closingDateUtc.AddDays(-1),
                cancellationToken))
            .ReturnsAsync((Domain.PortfolioValuations.PortfolioValuation?)null);

        var error = new Error("OP001", "Error obteniendo tipos de operación", ErrorType.Validation);

        operationTypesServiceMock
           .Setup(s => s.GetAllAsync(cancellationToken))
           .ReturnsAsync(Result.Failure<IReadOnlyCollection<OperationTypeInfo>>(error));

        var service = CreateService();

        // Act
        var result = await service.CalculateAndPersistValuationAsync(
            portfolioId,
            closingDateUtc,
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("OP001");

        portfolioValuationRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Domain.PortfolioValuations.PortfolioValuation>(), new CancellationToken()),
            Times.Never);
    }

    [Fact]
    public async Task CalculateAndPersistValuationAsyncReturnsFailureOnFirstDayWithoutIncomingOperations()
    {
        // Arrange
        var portfolioId = 30;
        var closingDateUtc = new DateTime(2024, 5, 10);
        var cancellationToken = CancellationToken.None;

        portfolioValuationRepositoryMock
            .Setup(r => r.ExistsByPortfolioAndDateAsync(
                portfolioId,
                closingDateUtc.Date,
                cancellationToken))
            .ReturnsAsync(false);

        portfolioValuationRepositoryMock
            .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(
                portfolioId,
                closingDateUtc.AddDays(-1),
                cancellationToken))
            .ReturnsAsync((Domain.PortfolioValuations.PortfolioValuation?)null);

        var operationTypes = new[]
        {
            new OperationTypeResponse(
                1,
                "SomeType",
                null,
                IncomeEgressNature.Income,
                Status.Active,
                "EXT",
                "H",
                JsonDocument.Parse("{}"))
        };


        operationTypesServiceMock
    .Setup(s => s.GetAllAsync(cancellationToken))
    .ReturnsAsync(Result.Success<IReadOnlyCollection<OperationTypeInfo>>(
        Array.Empty<OperationTypeInfo>()));

        var service = CreateService();

        // Act
        var result = await service.CalculateAndPersistValuationAsync(
            portfolioId,
            closingDateUtc,
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("002");
        result.Error!.Type.Should().Be(ErrorType.Validation);

  
        portfolioValuationRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Domain.PortfolioValuations.PortfolioValuation>(), new CancellationToken()),
            Times.Never);
    }

    [Fact]
    public async Task CalculateAndPersistValuationAsyncReturnsSuccessWhenPreviousValuationExists()
    {
        // Arrange
        var portfolioId = 40;
        var closingDateUtc = new DateTime(2024, 7, 15);
        var cancellationToken = CancellationToken.None;

        portfolioValuationRepositoryMock
            .Setup(r => r.ExistsByPortfolioAndDateAsync(
                portfolioId,
                closingDateUtc.Date,
                cancellationToken))
            .ReturnsAsync(false);

        var previousClosingDate = closingDateUtc.AddDays(-1);

        var previousValuationResult = Domain.PortfolioValuations.PortfolioValuation.Create(
            portfolioId,
            previousClosingDate,
            amount: 1_000m,
            initialValue: 1_000m,
            units: 100m,
            unitValue: 10m,
            grossYieldPerUnit: 0m,
            costPerUnit: 0m,
            dailyProfitability: 0m,
            incomingOperations: 0m,
            outgoingOperations: 0m,
            processDate: DateTime.UtcNow.AddDays(-1),
            isClosed: true);

        previousValuationResult.IsSuccess.Should().BeTrue();
        var previousValuation = previousValuationResult.Value;

        portfolioValuationRepositoryMock
            .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(
                portfolioId,
                previousClosingDate,
                cancellationToken))
            .ReturnsAsync(previousValuation);

        yieldRepositoryMock
            .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(
                portfolioId,
                closingDateUtc,
                cancellationToken))
            .ReturnsAsync((Yield?)null);

        var operationTypes = new[]
        {
            new OperationTypeResponse(
                1,
                "SomeType",
                null,
                IncomeEgressNature.Income,
                Status.Active,
                "EXT",
                "H",
                JsonDocument.Parse("{}"))
        };

        operationTypesServiceMock
        .Setup(s => s.GetAllAsync(cancellationToken))
        .ReturnsAsync(Result.Success<IReadOnlyCollection<OperationTypeInfo>>(
            Array.Empty<OperationTypeInfo>()));

        var service = CreateService();

        // Act
        var result = await service.CalculateAndPersistValuationAsync(
            portfolioId,
            closingDateUtc,
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var closedResult = result.Value;

        closedResult!.Income.Should().Be(0m);
        closedResult.Expenses.Should().Be(0m);
        closedResult.Commissions.Should().Be(0m);
        closedResult.Costs.Should().Be(0m);
        closedResult.YieldToCredit.Should().Be(0m);

        closedResult.UnitValue.Should().Be(10m);
        closedResult.DailyProfitability.Should().Be(0m);

        configurationParameterRepositoryMock.Verify(
            r => r.GetByUuidAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);


    }
}