using Common.SharedKernel.Domain;
using FluentAssertions;
using Moq;
using Reports.Application.LoadingInfo.Audit;
using Reports.Domain.LoadingInfo.Audit;
using Reports.Domain.LoadingInfo.Audit.Dto;
using System.Text.Json;

namespace Reports.test.UnitTests.Application.LoadingInfo.Audit;

public sealed class EtlExecutionServiceTests
{
    private readonly Mock<IEtlExecutionRepository> _mockRepository;
    private readonly EtlExecutionService _sut;

    public EtlExecutionServiceTests()
    {
        _mockRepository = new Mock<IEtlExecutionRepository>();
        _sut = new EtlExecutionService(_mockRepository.Object);
    }

    [Fact]
    public async Task StartAsync_WithValidParameters_ShouldReturnExecutionId()
    {
        // Arrange
        const string executionName = "loading-info";
        var builder = new ExecutionParametersBuilder("Products", new[] { "ETLProductos" })
        .WithCorrelationId("test-correlation-id")
        .WithPortfolioId(1);

        _mockRepository
        .Setup(x => x.InsertRunningAsync(
         executionName,
        It.IsAny<JsonDocument>(),
        It.IsAny<DateTimeOffset>(),
        It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(123L));

        // Act
        var result = await _sut.StartAsync(executionName, builder, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(123L);

        _mockRepository.Verify(x => x.InsertRunningAsync(
        executionName,
         It.IsAny<JsonDocument>(),
        It.IsAny<DateTimeOffset>(),
        It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartAsync_WithNullExecutionName_ShouldReturnFailure()
    {
        // Arrange
        var builder = new ExecutionParametersBuilder("Products", new[] { "ETLProductos" });

        // Act
        var result = await _sut.StartAsync(null!, builder, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ETL001");
        result.Error.Description.Should().Contain("executionName es requerido");

        _mockRepository.Verify(x => x.InsertRunningAsync(
        It.IsAny<string>(),
        It.IsAny<JsonDocument>(),
        It.IsAny<DateTimeOffset>(),
        It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task StartAsync_WithEmptyExecutionName_ShouldReturnFailure()
    {
        // Arrange
        var builder = new ExecutionParametersBuilder("Products", new[] { "ETLProductos" });

        // Act
        var result = await _sut.StartAsync(string.Empty, builder, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ETL001");
    }

    [Fact]
    public async Task StartAsync_WithNullBuilder_ShouldReturnFailure()
    {
        // Arrange
        const string executionName = "loading-info";

        // Act
        var result = await _sut.StartAsync(executionName, null!, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ETL003");
        result.Error.Description.Should().Contain("builder es requerido");
    }

    [Fact]
    public async Task CompleteAsync_WithValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        const long executionId = 456L;
        const long durationMs = 5000L;
        var builder = new ExecutionParametersBuilder("Products", new[] { "ETLProductos" })
        .WithCorrelationId("test-correlation-id");

        _mockRepository
        .Setup(x => x.FinalizeCompletedAsync(
         executionId,
         It.IsAny<DateTimeOffset>(),
         durationMs,
         It.IsAny<JsonDocument>(),
         It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.CompleteAsync(executionId, builder, durationMs, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockRepository.Verify(x => x.FinalizeCompletedAsync(
         executionId,
        It.IsAny<DateTimeOffset>(),
        durationMs,
         It.IsAny<JsonDocument>(),
         It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_WithInvalidExecutionId_ShouldReturnFailure()
    {
        // Arrange
        const long executionId = 0;
        var builder = new ExecutionParametersBuilder("Products", new[] { "ETLProductos" });

        // Act
        var result = await _sut.CompleteAsync(executionId, builder, 1000L, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ETL002");
        result.Error.Description.Should().Contain("debe ser un número positivo");
    }

    [Fact]
    public async Task CompleteAsync_WithNullBuilder_ShouldReturnFailure()
    {
        // Arrange
        const long executionId = 100L;

        // Act
        var result = await _sut.CompleteAsync(executionId, null!, 1000L, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ETL003");
    }

    [Fact]
    public async Task FailAsync_WithValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        const long executionId = 789L;
        const long durationMs = 3000L;
        var builder = new ExecutionParametersBuilder("Products", new[] { "ETLProductos" });
        var problemDetails = new ExecutionProblemDetailsDto(
           Type: "error",
           Title: "Error occurred",
            Status: 500,
            Code: "ERROR_001",
            Detail: "Something went wrong",
           Instance: "/test",
           TraceId: "trace-123",
           Extensions: null);

        _mockRepository
        .Setup(x => x.FinalizeFailedAsync(
         executionId,
         It.IsAny<DateTimeOffset>(),
        durationMs,
        It.IsAny<JsonDocument>(),
        It.IsAny<JsonDocument>(),
        It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.FailAsync(executionId, builder, durationMs, problemDetails, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockRepository.Verify(x => x.FinalizeFailedAsync(
        executionId,
        It.IsAny<DateTimeOffset>(),
        durationMs,
        It.IsAny<JsonDocument>(),
        It.IsAny<JsonDocument>(),
        It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FailAsync_WithInvalidExecutionId_ShouldReturnFailure()
    {
        // Arrange
        const long executionId = -1;
        var builder = new ExecutionParametersBuilder("Products", new[] { "ETLProductos" });
        var problemDetails = new ExecutionProblemDetailsDto(
         Type: "error",
        Title: "Error",
         Status: 500,
         Code: "ERROR_001",
        Detail: "Error",
         Instance: "/test",
         TraceId: "trace-123",
          Extensions: null);

        // Act
        var result = await _sut.FailAsync(executionId, builder, 1000L, problemDetails, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ETL002");
    }

    [Fact]
    public async Task FailAsync_WithNullBuilder_ShouldReturnFailure()
    {
        // Arrange
        const long executionId = 100L;
        var problemDetails = new ExecutionProblemDetailsDto(
            Type: "error",
            Title: "Error",
           Status: 500,
           Code: "ERROR_001",
           Detail: "Error",
           Instance: "/test",
           TraceId: "trace-123",
           Extensions: null);

        // Act
        var result = await _sut.FailAsync(executionId, null!, 1000L, problemDetails, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ETL003");
    }

    [Fact]
    public async Task FailAsync_WithNullProblemDetails_ShouldReturnFailure()
    {
        // Arrange
        const long executionId = 100L;
        var builder = new ExecutionParametersBuilder("Products", new[] { "ETLProductos" });

        // Act
        var result = await _sut.FailAsync(executionId, builder, 1000L, null!, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ETL005");
        result.Error.Description.Should().Contain("problemDetails es requerido");
    }
}