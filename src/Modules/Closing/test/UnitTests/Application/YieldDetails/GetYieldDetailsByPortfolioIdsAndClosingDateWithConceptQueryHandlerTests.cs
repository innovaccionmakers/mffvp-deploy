using Closing.Application.YieldDetails;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.YieldDetails;
using Closing.Integrations.YieldDetails.Queries;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;

namespace Closing.test.UnitTests.Application.YieldDetails;

public class GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptQueryHandlerTests
{
    private readonly Mock<ILogger<GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptQueryHandler>> _mockLogger;
    private readonly Mock<IConfigurationParameterRepository> _mockConfigurationParameterRepository;
    private readonly Mock<IYieldDetailRepository> _mockYieldDetailRepository;
    private readonly GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptQueryHandler _handler;

    public GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptQueryHandler>>();
        _mockConfigurationParameterRepository = new Mock<IConfigurationParameterRepository>();
        _mockYieldDetailRepository = new Mock<IYieldDetailRepository>();
        _handler = new GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptQueryHandler(
            _mockLogger.Object,
            _mockConfigurationParameterRepository.Object,
            _mockYieldDetailRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidConcepts_ReturnsSuccessResult()
    {
        // Arrange
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        var query = new GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptQuery(
            PortfolioIds: new[] { 123, 456 },
            ClosingDate: new DateTime(2024, 1, 15),
            Source: "TestSource",
            GuidConcepts: new[] { guid1, guid2 }
        );

        var conceptParam1 = CreateConfigurationParameter(guid1, "{\"id\": 1, \"nombre\": \"Concepto 1\"}");
        var conceptParam2 = CreateConfigurationParameter(guid2, "{\"id\": 2, \"nombre\": \"Concepto 2\"}");

        _mockConfigurationParameterRepository
            .Setup(repo => repo.GetReadOnlyByUuidsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ConfigurationParameter>
            {
                { guid1, conceptParam1 },
                { guid2, conceptParam2 }
            });

        var yieldDetails = new List<YieldDetail>
        {
            CreateYieldDetail(123, new DateTime(2024, 1, 15), 100m, 50m, 10m),
            CreateYieldDetail(456, new DateTime(2024, 1, 15), 200m, 100m, 20m)
        };

        _mockYieldDetailRepository
            .Setup(repo => repo.GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptsAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(yieldDetails);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(300m, result.Value.Sum(yd => yd.Income));
        Assert.Equal(150m, result.Value.Sum(yd => yd.Expenses));
        Assert.Equal(30m, result.Value.Sum(yd => yd.Commissions));
    }

    [Fact]
    public async Task Handle_WithNoResults_ReturnsFailure()
    {
        // Arrange
        var guid1 = Guid.NewGuid();
        var query = new GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptQuery(
            PortfolioIds: new[] { 123 },
            ClosingDate: new DateTime(2024, 1, 15),
            Source: "TestSource",
            GuidConcepts: new[] { guid1 }
        );

        var conceptParam1 = CreateConfigurationParameter(guid1, "{\"id\": 1, \"nombre\": \"Concepto 1\"}");

        _mockConfigurationParameterRepository
            .Setup(repo => repo.GetReadOnlyByUuidsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ConfigurationParameter> { { guid1, conceptParam1 } });

        _mockYieldDetailRepository
            .Setup(repo => repo.GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptsAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<YieldDetail>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Error", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithInvalidConceptMetadata_SkipsInvalidConcepts()
    {
        // Arrange
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        var query = new GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptQuery(
            PortfolioIds: new[] { 123 },
            ClosingDate: new DateTime(2024, 1, 15),
            Source: "TestSource",
            GuidConcepts: new[] { guid1, guid2 }
        );

        var conceptParam1 = CreateConfigurationParameter(guid1, "{\"id\": 1, \"nombre\": \"Concepto 1\"}");
        var conceptParam2 = CreateConfigurationParameter(guid2, "{}"); // Sin metadata válido

        _mockConfigurationParameterRepository
            .Setup(repo => repo.GetReadOnlyByUuidsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ConfigurationParameter>
            {
                { guid1, conceptParam1 },
                { guid2, conceptParam2 }
            });

        var yieldDetails = new List<YieldDetail>
        {
            CreateYieldDetail(123, new DateTime(2024, 1, 15), 100m, 50m, 10m)
        };

        _mockYieldDetailRepository
            .Setup(repo => repo.GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptsAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(yieldDetails);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);
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

    private static YieldDetail CreateYieldDetail(int portfolioId, DateTime closingDate, decimal income, decimal expenses, decimal commissions)
    {
        var concept = JsonDocument.Parse("{\"EntityId\": \"1\", \"EntityValue\": \"Test Concept\"}");
        var result = YieldDetail.Create(
            portfolioId,
            closingDate,
            "TestSource",
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
}

