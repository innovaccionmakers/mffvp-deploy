using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Reports.Application.LoadingInfo.Contracts;
using Reports.Application.LoadingInfo.Audit;
using Reports.Application.LoadingInfo.Orchestration;
using Reports.Domain.LoadingInfo.Constants;

namespace Reports.test.UnitTests.Application.LoadingInfo.Orchestration;

public sealed class LoadingInfoOrchestratorTests
{
    private readonly Mock<IPeopleLoader> _mockPeopleLoader;
    private readonly Mock<IProductsLoader> _mockProductsLoader;
    private readonly Mock<IClosingLoader> _mockClosingLoader;
    private readonly Mock<IBalancesLoader> _mockBalancesLoader;
    private readonly Mock<IEtlAuditRunner> _mockEtlAuditRunner;
    private readonly Mock<ILogger<LoadingInfoOrchestrator>> _mockLogger;
    private readonly LoadingInfoOrchestrator _sut;

    public LoadingInfoOrchestratorTests()
    {
        _mockPeopleLoader = new Mock<IPeopleLoader>();
        _mockProductsLoader = new Mock<IProductsLoader>();
        _mockClosingLoader = new Mock<IClosingLoader>();
        _mockBalancesLoader = new Mock<IBalancesLoader>();
        _mockEtlAuditRunner = new Mock<IEtlAuditRunner>();
        _mockLogger = new Mock<ILogger<LoadingInfoOrchestrator>>();

        _sut = new LoadingInfoOrchestrator(
            _mockPeopleLoader.Object,
            _mockProductsLoader.Object,
            _mockClosingLoader.Object,
            _mockBalancesLoader.Object,
            _mockEtlAuditRunner.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task RunAsync_WithNoSelection_ShouldReturnValidationFailure()
    {
        // Arrange
        const string executionId = "test-correlation-id";
        const EtlSelection selection = EtlSelection.None;
        var closingDate = new DateTime(2024, 1, 31);
        const int portfolioId = 1;

        // Act
        var result = await _sut.RunAsync(executionId, selection, closingDate, portfolioId, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ETLSEL_001");
        result.Error.Description.Should().Contain("Se debe seleccionar al menos un ETL");
    }

    [Fact]
    public async Task RunAsync_WithInvalidPortfolioId_ShouldReturnValidationFailure()
    {
        // Arrange
        const string executionId = "test-correlation-id";
        const EtlSelection selection = EtlSelection.Products;
        var closingDate = new DateTime(2024, 1, 31);
        const int portfolioId = 0; // Invalid

        // Act
        var result = await _sut.RunAsync(executionId, selection, closingDate, portfolioId, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ETLSEL_002");
        result.Error.Description.Should().Contain("PortfolioId debe ser un número entero positivo");
    }
}