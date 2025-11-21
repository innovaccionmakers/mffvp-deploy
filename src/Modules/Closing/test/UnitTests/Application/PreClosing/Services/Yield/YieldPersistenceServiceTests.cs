using Closing.Application.Closing.Services.Warnings;
using Closing.Application.PreClosing.Services.Yield;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.YieldDetails;
using Closing.Domain.Yields;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Exceptions;
using Common.SharedKernel.Domain.ConfigurationParameters;
using FluentAssertions;
using Moq;
using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace Closing.test.UnitTests.Application.PreClosing.Services.Yields;

public class YieldPersistenceServiceTests
{
    private readonly Mock<IYieldDetailRepository> yieldDetailRepositoryMock = new();
    private readonly Mock<IYieldRepository> yieldRepositoryMock = new();
    private readonly Mock<IPortfolioValuationRepository> portfolioValuationRepositoryMock = new();
    private readonly Mock<IConfigurationParameterRepository> configurationParameterRepositoryMock = new();
    private readonly Mock<IWarningCollector> warningCollectorMock = new();

    private YieldPersistenceService CreateService()
        => new(
            yieldDetailRepositoryMock.Object,
            yieldRepositoryMock.Object,
            portfolioValuationRepositoryMock.Object,
            configurationParameterRepositoryMock.Object,
            warningCollectorMock.Object);

    [Fact]
    public async Task ConsolidateAsyncThrowsWhenThereAreNoYieldDetails()
    {
        // Arrange
        var parameters = CreateParameters(isClosing: false, isFirstClosingDay: false);

        yieldDetailRepositoryMock
            .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(
                parameters.PortfolioId,
                parameters.ClosingDate,
                parameters.IsClosing,
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<IReadOnlyCollection<YieldDetail>>(
                Array.Empty<YieldDetail>()));

        var service = CreateService();

        // Act
        Func<Task> act = () => service.ConsolidateAsync(parameters, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<BusinessRuleValidationException>()
            .WithMessage("No hay detalles de rendimiento para consolidar.");

        yieldRepositoryMock.Verify(
            r => r.InsertAsync(It.IsAny<Yield>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ConsolidateAsyncForClosingAggregatesAndPersistsYieldAndSkipsSimulationValues()
    {
        // Arrange
        var parameters = CreateParameters(isClosing: true, isFirstClosingDay: false);

        var details = new[]
        {
                CreateYieldDetail(100m, 20m, 5m),
                CreateYieldDetail(50m, 10m, 5m)
            };

        yieldDetailRepositoryMock
            .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(
                parameters.PortfolioId,
                parameters.ClosingDate,
                parameters.IsClosing,
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<IReadOnlyCollection<YieldDetail>>(details));

        var service = CreateService();

        // Act
        var result = await service.ConsolidateAsync(parameters, CancellationToken.None);

        // Assert: se inserta un Yield
        yieldRepositoryMock.Verify(
            r => r.InsertAsync(It.IsAny<Yield>(), It.IsAny<CancellationToken>()),
            Times.Once);

        configurationParameterRepositoryMock.Verify(
            r => r.GetByUuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        portfolioValuationRepositoryMock.Verify(
            r => r.GetReadOnlyByPortfolioAndDateAsync(
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        result.Income.Should().Be(150m);
        result.Expenses.Should().Be(30m);
        result.Commissions.Should().Be(10m);
        result.Costs.Should().Be(40m);
        result.YieldToCredit.Should().Be(110m);

        result.UnitValue.Should().BeNull();
        result.DailyProfitability.Should().BeNull();
    }

    [Fact]
    public async Task ConsolidateAsyncWhenFirstClosingDayUsesInitialFundUnitValueAndSkipsPreviousValuation()
    {
        // Arrange
        var parameters = CreateParameters(isClosing: false, isFirstClosingDay: true);

        var details = new[]
        {
                CreateYieldDetail(100m, 20m, 5m),
                CreateYieldDetail(50m, 10m, 5m)
            };

        yieldDetailRepositoryMock
            .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(
                parameters.PortfolioId,
                parameters.ClosingDate,
                parameters.IsClosing,
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<IReadOnlyCollection<YieldDetail>>(details));

        var configurationParameter = CreateConfigurationParameterWithUnitValue(10m);

        configurationParameterRepositoryMock
            .Setup(r => r.GetByUuidAsync(
                ConfigurationParameterUuids.Closing.InitialFundUnitValue,
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(configurationParameter));

        var service = CreateService();

        // Act
        var result = await service.ConsolidateAsync(parameters, CancellationToken.None);

        // Assert
        configurationParameterRepositoryMock.Verify(
            r => r.GetByUuidAsync(
                ConfigurationParameterUuids.Closing.InitialFundUnitValue,
                It.IsAny<CancellationToken>()),
            Times.Once);

        portfolioValuationRepositoryMock.Verify(
            r => r.GetReadOnlyByPortfolioAndDateAsync(
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        result.DailyProfitability.Should().BeNull();
        result.UnitValue.Should().Be(10m);

        result.Income.Should().Be(150m);
        result.Expenses.Should().Be(30m);
        result.Commissions.Should().Be(10m);
    }

    [Fact]
    public async Task ConsolidateAsyncWhenPreviousValuationIsMissingShouldReturnNullSimulationValues()
    {
        // Arrange
        var parameters = CreateParameters(isClosing: false, isFirstClosingDay: false);

        var details = new[]
        {
                CreateYieldDetail(100m, 20m, 5m)
            };

        yieldDetailRepositoryMock
            .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(
                parameters.PortfolioId,
                parameters.ClosingDate,
                parameters.IsClosing,
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<IReadOnlyCollection<YieldDetail>>(details));

        portfolioValuationRepositoryMock
            .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(
                parameters.PortfolioId,
                parameters.ClosingDate.AddDays(-1),
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<PortfolioValuation?>(null));

        var service = CreateService();

        // Act
        var result = await service.ConsolidateAsync(parameters, CancellationToken.None);

        // Assert
        portfolioValuationRepositoryMock.Verify(
            r => r.GetReadOnlyByPortfolioAndDateAsync(
                parameters.PortfolioId,
                parameters.ClosingDate.AddDays(-1),
                It.IsAny<CancellationToken>()),
            Times.Once);

        result.UnitValue.Should().BeNull();
        result.DailyProfitability.Should().BeNull();

    }

    [Fact]
    public async Task ConsolidateAsyncWhenPreviousValuationIsValidShouldReturnSimulationValues()
    {
        // Arrange
        var parameters = CreateParameters(isClosing: false, isFirstClosingDay: false);

        var details = new[]
        {
                CreateYieldDetail(200m, 50m, 20m)
            };

        yieldDetailRepositoryMock
            .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(
                parameters.PortfolioId,
                parameters.ClosingDate,
                parameters.IsClosing,
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<IReadOnlyCollection<YieldDetail>>(details));

        var previousValuation = CreatePortfolioValuation(
            amount: 1_000m,
            unitValue: 10m,
            units: 100m);

        portfolioValuationRepositoryMock
            .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(
                parameters.PortfolioId,
                parameters.ClosingDate.AddDays(-1),
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<PortfolioValuation?>(previousValuation));

        var service = CreateService();

        // Act
        var result = await service.ConsolidateAsync(parameters, CancellationToken.None);

        // Assert
        portfolioValuationRepositoryMock.Verify(
            r => r.GetReadOnlyByPortfolioAndDateAsync(
                parameters.PortfolioId,
                parameters.ClosingDate.AddDays(-1),
                It.IsAny<CancellationToken>()),
            Times.Once);

        configurationParameterRepositoryMock.Verify(
            r => r.GetByUuidAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        result.UnitValue.Should().NotBeNull();
        result.DailyProfitability.Should().NotBeNull();

        result.UnitValue!.Value.Should().BeGreaterThan(0m);
        result.DailyProfitability!.Value.Should().BeGreaterThan(-100m)
            .And.BeLessThan(100m);
    }

    #region Helpers

    private static RunSimulationParameters CreateParameters(bool isClosing, bool isFirstClosingDay)
    {
        return new RunSimulationParameters
        (
            123,
            new DateTime(2025, 1, 10),
            isClosing,
            isFirstClosingDay,
            false
        );
    }

    private static YieldDetail CreateYieldDetail(decimal income, decimal expenses, decimal commissions)
    {
        var concept = JsonDocument.Parse(@"{ ""concept"": ""test"" }");

        var result = YieldDetail.Create(
            portfolioId: 123,
            closingDate: new DateTime(2025, 1, 10),
            source: "TestSource",
            concept: concept,
            income: income,
            expenses: expenses,
            commissions: commissions,
            processDate: DateTime.UtcNow,
            isClosed: false);

        if (result.IsFailure)
            throw new InvalidOperationException("Falló la creación de YieldDetail para pruebas.");

        return result.Value;
    }

    private static PortfolioValuation CreatePortfolioValuation(decimal amount, decimal unitValue, decimal units)
    {
        var result = PortfolioValuation.Create(
            portfolioId: 123,
            closingDate: new DateTime(2025, 1, 9),
            amount: amount,
            initialValue: amount,
            units: units,
            unitValue: unitValue,
            grossYieldPerUnit: 0m,
            costPerUnit: 0m,
            dailyProfitability: 0m,
            incomingOperations: 0m,
            outgoingOperations: 0m,
            processDate: DateTime.UtcNow,
            isClosed: true);

        if (result.IsFailure)
            throw new InvalidOperationException("Falló la creación de PortfolioValuation para pruebas.");

        return result.Value;
    }

    private static ConfigurationParameter CreateConfigurationParameterWithUnitValue(decimal unitValue)
    {
        var type = typeof(ConfigurationParameter);
        var instance = (ConfigurationParameter)Activator.CreateInstance(type, nonPublic: true)!;

        var json = JsonDocument.Parse(
            $@"{{ ""valor"": {unitValue.ToString(CultureInfo.InvariantCulture)} }}");

        var property = type.GetProperty(
            "Metadata",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (property is not null && property.CanWrite)
        {
            property.SetValue(instance, json);
            return instance;
        }

        var backingField = type.GetField(
            "<Metadata>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);

        backingField?.SetValue(instance, json);

        return instance;
    }

    #endregion
}