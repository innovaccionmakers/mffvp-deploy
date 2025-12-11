using Closing.Application.Yields;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.YieldDetails;
using Closing.Domain.Yields;
using Closing.Domain.YieldsToDistribute;
using Closing.Integrations.Yields.Queries;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;

namespace Closing.test.UnitTests.Application.Yields;

public class GetAllAutConceptsQueryHandlerTests
{
    private readonly Mock<ILogger<GetAllFeesQueryHandler>> _mockLogger;
    private readonly Mock<IYieldRepository> _mockYieldRepository;
    private readonly Mock<IYieldDetailRepository> _mockYieldDetailRepository;
    private readonly Mock<IYieldToDistributeRepository> _mockYieldToDistributeRepository;
    private readonly Mock<IConfigurationParameterRepository> _mockConfigurationParameterRepository;
    private readonly GetAllAutConceptsQueryHandler _handler;

    public GetAllAutConceptsQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GetAllFeesQueryHandler>>();
        _mockYieldRepository = new Mock<IYieldRepository>();
        _mockYieldDetailRepository = new Mock<IYieldDetailRepository>();
        _mockYieldToDistributeRepository = new Mock<IYieldToDistributeRepository>();
        _mockConfigurationParameterRepository = new Mock<IConfigurationParameterRepository>();
        _handler = new GetAllAutConceptsQueryHandler(
            _mockLogger.Object,
            _mockYieldRepository.Object,
            _mockYieldDetailRepository.Object,
            _mockYieldToDistributeRepository.Object,
            _mockConfigurationParameterRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var portfolioIds = new[] { 1, 2 };
        var closingDate = new DateTime(2024, 1, 15);
        var query = new GetAllAutConceptsQuery(portfolioIds, closingDate);

        var yields = new List<Yield>
        {
            CreateYield(1, 1, 100m, 50m, closingDate),
            CreateYield(2, 2, 200m, 100m, closingDate)
        };

        var yieldDetails = new List<YieldDetail>
        {
            CreateYieldDetail(1, closingDate, 150m, 75m, 10m),
            CreateYieldDetail(2, closingDate, 250m, 125m, 20m)
        };

        var distributedYields = new List<YieldToDistribute>
        {
            CreateYieldToDistribute(1, closingDate, 30m),
            CreateYieldToDistribute(2, closingDate, 40m)
        };

        SetupMocks(yields, yieldDetails, distributedYields);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Yields.Count);
        Assert.Equal(2, result.Value.YieldDetails.Count);
        Assert.Equal(100m, result.Value.Yields.First(y => y.PortfolioId == 1).YieldToCredit);
        Assert.Equal(30m, result.Value.Yields.First(y => y.PortfolioId == 1).YieldToDistributedValue);
        Assert.Equal(40m, result.Value.Yields.First(y => y.PortfolioId == 2).YieldToDistributedValue);
    }

    [Fact]
    public async Task Handle_WithNoYields_ReturnsFailure()
    {
        // Arrange
        var portfolioIds = new[] { 1 };
        var closingDate = new DateTime(2024, 1, 15);
        var query = new GetAllAutConceptsQuery(portfolioIds, closingDate);

        _mockYieldRepository
            .Setup(repo => repo.GetAllAutConceptsByPortfolioIdsAndClosingDateAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Yield>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Error", result.Error.Code);
        Assert.Contains("No se encontraron rendimientos", result.Error.Description);
    }

    [Fact]
    public async Task Handle_WithNullYields_ReturnsFailure()
    {
        // Arrange
        var portfolioIds = new[] { 1 };
        var closingDate = new DateTime(2024, 1, 15);
        var query = new GetAllAutConceptsQuery(portfolioIds, closingDate);

        _mockYieldRepository
            .Setup(repo => repo.GetAllAutConceptsByPortfolioIdsAndClosingDateAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Yield>?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Error", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithNoDistributedYields_ReturnsZeroForYieldToDistributedValue()
    {
        // Arrange
        var portfolioIds = new[] { 1 };
        var closingDate = new DateTime(2024, 1, 15);
        var query = new GetAllAutConceptsQuery(portfolioIds, closingDate);

        var yields = new List<Yield>
        {
            CreateYield(1, 1, 100m, 50m, closingDate)
        };

        var yieldDetails = new List<YieldDetail>
        {
            CreateYieldDetail(1, closingDate, 150m, 75m, 10m)
        };

        SetupMocks(yields, yieldDetails, new List<YieldToDistribute>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(0m, result.Value.Yields.First().YieldToDistributedValue);
    }

    [Fact]
    public async Task Handle_WithMultipleDistributedYieldsForSamePortfolio_SumsCorrectly()
    {
        // Arrange
        var portfolioIds = new[] { 1 };
        var closingDate = new DateTime(2024, 1, 15);
        var query = new GetAllAutConceptsQuery(portfolioIds, closingDate);

        var yields = new List<Yield>
        {
            CreateYield(1, 1, 100m, 50m, closingDate)
        };

        var yieldDetails = new List<YieldDetail>
        {
            CreateYieldDetail(1, closingDate, 150m, 75m, 10m)
        };

        var distributedYields = new List<YieldToDistribute>
        {
            CreateYieldToDistribute(1, closingDate, 30m),
            CreateYieldToDistribute(1, closingDate, 20m)
        };

        SetupMocks(yields, yieldDetails, distributedYields);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(50m, result.Value.Yields.First().YieldToDistributedValue);
    }

    [Fact]
    public async Task Handle_WithException_ReturnsFailure()
    {
        // Arrange
        var portfolioIds = new[] { 1 };
        var closingDate = new DateTime(2024, 1, 15);
        var query = new GetAllAutConceptsQuery(portfolioIds, closingDate);

        _mockYieldRepository
            .Setup(repo => repo.GetAllAutConceptsByPortfolioIdsAndClosingDateAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Error", result.Error.Code);
        Assert.Equal(ErrorType.Problem, result.Error.Type);
    }

    [Fact]
    public async Task Handle_VerifiesRepositoriesAreCalledCorrectly()
    {
        // Arrange
        var portfolioIds = new[] { 1, 2 };
        var closingDate = new DateTime(2024, 1, 15);
        var query = new GetAllAutConceptsQuery(portfolioIds, closingDate);

        var yields = new List<Yield>
        {
            CreateYield(1, 1, 100m, 50m, closingDate)
        };

        var yieldDetails = new List<YieldDetail>
        {
            CreateYieldDetail(1, closingDate, 150m, 75m, 10m)
        };

        SetupMocks(yields, yieldDetails, new List<YieldToDistribute>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockYieldRepository.Verify(
            repo => repo.GetAllAutConceptsByPortfolioIdsAndClosingDateAsync(
                portfolioIds,
                closingDate,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockYieldDetailRepository.Verify(
            repo => repo.GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptsAsync(
                portfolioIds,
                closingDate,
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockYieldToDistributeRepository.Verify(
            repo => repo.GetDistributedYieldsByConceptAsync(
                portfolioIds,
                closingDate,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void SetupMocks(
        List<Yield> yields,
        List<YieldDetail> yieldDetails,
        List<YieldToDistribute> distributedYields)
    {
        _mockYieldRepository
            .Setup(repo => repo.GetAllAutConceptsByPortfolioIdsAndClosingDateAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(yields);

        var incomeGuid = ConfigurationParameterUuids.Closing.YieldAdjustmentIncome;
        var expenseGuid = ConfigurationParameterUuids.Closing.YieldAdjustmentExpense;
        var creditNoteGuid = ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNote;

        var incomeParam = CreateConfigurationParameter(incomeGuid, "{\"id\": 1, \"nombre\": \"Ajuste Ingreso\"}");
        var expenseParam = CreateConfigurationParameter(expenseGuid, "{\"id\": 2, \"nombre\": \"Ajuste Gasto\"}");
        var creditNoteParam = CreateConfigurationParameter(creditNoteGuid, "{\"id\": 3, \"nombre\": \"Ajuste Nota Contable\"}");

        _mockConfigurationParameterRepository
            .Setup(repo => repo.GetReadOnlyByUuidsAsync(
                It.Is<IEnumerable<Guid>>(g => g.Contains(incomeGuid) && g.Contains(expenseGuid)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ConfigurationParameter>
            {
                { incomeGuid, incomeParam },
                { expenseGuid, expenseParam }
            });

        _mockConfigurationParameterRepository
            .Setup(repo => repo.GetReadOnlyByUuidsAsync(
                It.Is<IEnumerable<Guid>>(g => g.Contains(creditNoteGuid)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ConfigurationParameter>
            {
                { creditNoteGuid, creditNoteParam }
            });

        _mockYieldDetailRepository
            .Setup(repo => repo.GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptsAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(yieldDetails);

        _mockYieldToDistributeRepository
            .Setup(repo => repo.GetDistributedYieldsByConceptAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(distributedYields);
    }

    private static Yield CreateYield(long yieldId, int portfolioId, decimal yieldToCredit, decimal creditedYields, DateTime closingDate)
    {
        var yield = Yield.Create(
            portfolioId,
            income: 100m,
            expenses: 50m,
            commissions: 10m,
            costs: 5m,
            yieldToCredit,
            creditedYields,
            closingDate,
            processDate: DateTime.UtcNow,
            isClosed: true);

        if (yield.IsFailure)
            throw new InvalidOperationException("Falló la creación de Yield para pruebas.");

        var instance = yield.Value;
        var type = typeof(Yield);
        var yieldIdProp = type.GetProperty("YieldId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        yieldIdProp?.SetValue(instance, yieldId);

        return instance;
    }

    private static YieldDetail CreateYieldDetail(int portfolioId, DateTime closingDate, decimal income, decimal expenses, decimal commissions)
    {
        var concept = JsonDocument.Parse("{\"EntityId\": \"1\", \"EntityValue\": \"Test Concept\"}");
        var result = YieldDetail.Create(
            portfolioId,
            closingDate,
            "AutomaticConcept",
            concept,
            income,
            expenses,
            commissions,
            DateTime.UtcNow,
            true);

        if (result.IsFailure)
            throw new InvalidOperationException("Falló la creación de YieldDetail para pruebas.");

        return result.Value;
    }

    private static YieldToDistribute CreateYieldToDistribute(int portfolioId, DateTime closingDate, decimal yieldAmount)
    {
        var concept = JsonDocument.Parse("{\"EntityId\": \"3\", \"EntityValue\": \"Ajuste Nota Contable\"}");
        var result = YieldToDistribute.Create(
            trustId: 1,
            portfolioId,
            closingDate,
            applicationDate: DateTime.UtcNow,
            participation: 1m,
            yieldAmount,
            concept,
            processDate: DateTime.UtcNow);

        if (result.IsFailure)
            throw new InvalidOperationException("Falló la creación de YieldToDistribute para pruebas.");

        return result.Value;
    }

    private static ConfigurationParameter CreateConfigurationParameter(Guid uuid, string metadataJson)
    {
        var param = (ConfigurationParameter)FormatterServices.GetUninitializedObject(typeof(ConfigurationParameter));
        var uuidProp = typeof(ConfigurationParameter).GetProperty(nameof(ConfigurationParameter.Uuid),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var metaProp = typeof(ConfigurationParameter).GetProperty(nameof(ConfigurationParameter.Metadata),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        uuidProp?.SetValue(param, uuid);
        metaProp?.SetValue(param, JsonDocument.Parse(metadataJson));

        return param;
    }
}

