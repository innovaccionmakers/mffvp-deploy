using Common.SharedKernel.Domain;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Reports.Application.LoadingInfo.Features.Products;
using Reports.Application.LoadingInfo.Models;
using Reports.Domain.LoadingInfo.Audit;
using Reports.Domain.LoadingInfo.Products;

namespace Reports.test.UnitTests.Application.LoadingInfo.Features.Products;

public sealed class EtlProductsServiceTests
{
    private readonly Mock<IProductSheetReadRepository> _mockReadRepository;
    private readonly Mock<IProductSheetWriteRepository> _mockWriteRepository;
    private readonly Mock<IEtlExecutionRepository> _mockExecutionRepository;
    private readonly Mock<ILogger<EtlProductsService>> _mockLogger;
    private readonly EtlProductsService _sut;

    public EtlProductsServiceTests()
    {
        _mockReadRepository = new Mock<IProductSheetReadRepository>();
        _mockWriteRepository = new Mock<IProductSheetWriteRepository>();
        _mockExecutionRepository = new Mock<IEtlExecutionRepository>();
        _mockLogger = new Mock<ILogger<EtlProductsService>>();

        _sut = new EtlProductsService(
         _mockReadRepository.Object,
        _mockWriteRepository.Object,
        _mockExecutionRepository.Object,
          _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_FirstExecution_ShouldTruncateAndInsertAllData()
    {
        // Arrange
        _mockExecutionRepository
            .Setup(x => x.GetLastSuccessfulExecutionTimestampAsync("loading-info", It.IsAny<CancellationToken>()))
            .ReturnsAsync((long?)null);

        var sourceData = new List<ProductSheetReadRow>
        {
         new(AdministratorId: 1, EntityType: 1, EntityCode: "Code1", EntitySfcCode: "SFC1", BusinessCodeSfcFund: 100, FundId: 100),
        new(AdministratorId: 2, EntityType: 2, EntityCode: "Code2", EntitySfcCode: "SFC2", BusinessCodeSfcFund: 200, FundId: 200)
        };

        _mockReadRepository
        .Setup(x => x.ReadProductsAsync(null, It.IsAny<CancellationToken>()))
         .Returns(sourceData.ToAsyncEnumerable());

        _mockWriteRepository
             .Setup(x => x.TruncateAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockWriteRepository
        .Setup(x => x.BulkInsertAsync(It.IsAny<IReadOnlyList<ProductSheet>>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ReadRows.Should().Be(2);
        result.Value.InsertedRows.Should().Be(2);

        _mockWriteRepository.Verify(x => x.TruncateAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockWriteRepository.Verify(x => x.DeleteByFundIdsAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockWriteRepository.Verify(x => x.BulkInsertAsync(It.Is<IReadOnlyList<ProductSheet>>(list => list.Count == 2), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_IncrementalLoad_ShouldDeleteAndInsertUpdatedData()
    {
        // Arrange
        var lastRowVersion = 1234567890L;

        _mockExecutionRepository
     .Setup(x => x.GetLastSuccessfulExecutionTimestampAsync("loading-info", It.IsAny<CancellationToken>()))
     .ReturnsAsync(lastRowVersion);

        var sourceData = new List<ProductSheetReadRow>
        {
          new(AdministratorId: 1, EntityType: 1, EntityCode: "Code1", EntitySfcCode: "SFC1", BusinessCodeSfcFund: 100, FundId: 100),
          new(AdministratorId: 2, EntityType: 2, EntityCode: "Code2", EntitySfcCode: "SFC2", BusinessCodeSfcFund: 200, FundId: 200)
         };

        _mockReadRepository
            .Setup(x => x.ReadProductsAsync(lastRowVersion, It.IsAny<CancellationToken>()))
          .Returns(sourceData.ToAsyncEnumerable());

        _mockWriteRepository
        .Setup(x => x.DeleteByFundIdsAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

        _mockWriteRepository
                .Setup(x => x.BulkInsertAsync(It.IsAny<IReadOnlyList<ProductSheet>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ReadRows.Should().Be(2);
        result.Value.InsertedRows.Should().Be(2);

        _mockWriteRepository.Verify(x => x.TruncateAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockWriteRepository.Verify(x => x.DeleteByFundIdsAsync(
        It.Is<IReadOnlyCollection<int>>(ids => ids.Contains(100) && ids.Contains(200) && ids.Count == 2),
        It.IsAny<CancellationToken>()), Times.Once);
        _mockWriteRepository.Verify(x => x.BulkInsertAsync(It.Is<IReadOnlyList<ProductSheet>>(list => list.Count == 2), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NoDataToProcess_ShouldReturnZeroMetrics()
    {
        // Arrange
        _mockExecutionRepository
          .Setup(x => x.GetLastSuccessfulExecutionTimestampAsync("loading-info", It.IsAny<CancellationToken>()))
           .ReturnsAsync((long?)null);

        var sourceData = new List<ProductSheetReadRow>();

        _mockReadRepository
          .Setup(x => x.ReadProductsAsync(null, It.IsAny<CancellationToken>()))
           .Returns(sourceData.ToAsyncEnumerable());

        _mockWriteRepository
      .Setup(x => x.TruncateAsync(It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ReadRows.Should().Be(0);
        result.Value.InsertedRows.Should().Be(0);

        _mockWriteRepository.Verify(x => x.TruncateAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockWriteRepository.Verify(x => x.BulkInsertAsync(It.IsAny<IReadOnlyList<ProductSheet>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        _mockExecutionRepository
 .Setup(x => x.GetLastSuccessfulExecutionTimestampAsync("loading-info", It.IsAny<CancellationToken>()))
  .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ETL_PRODUCTS_001");
        result.Error.Description.Should().Contain("Database error");
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockExecutionRepository
            .Setup(x => x.GetLastSuccessfulExecutionTimestampAsync("loading-info", It.IsAny<CancellationToken>()))
          .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => _sut.ExecuteAsync(cts.Token));
    }

    [Fact]
    public async Task ExecuteAsync_LargeDataSet_ShouldProcessInBatches()
    {
        // Arrange
        _mockExecutionRepository
       .Setup(x => x.GetLastSuccessfulExecutionTimestampAsync("loading-info", It.IsAny<CancellationToken>()))
        .ReturnsAsync((long?)null);

        // Create 15000 records to ensure multiple batches (batch size is 10000)
        var sourceData = Enumerable.Range(1, 15000)
         .Select(i => new ProductSheetReadRow(
             AdministratorId: i,
             EntityType: i,
             EntityCode: $"Code{i}",
             EntitySfcCode: $"SFC{i}",
             BusinessCodeSfcFund: i,
             FundId: i))
            .ToList();

        _mockReadRepository
          .Setup(x => x.ReadProductsAsync(null, It.IsAny<CancellationToken>()))
            .Returns(sourceData.ToAsyncEnumerable());

        _mockWriteRepository
          .Setup(x => x.TruncateAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockWriteRepository
            .Setup(x => x.BulkInsertAsync(It.IsAny<IReadOnlyList<ProductSheet>>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ReadRows.Should().Be(15000);
        result.Value.InsertedRows.Should().Be(15000);

        // Should be called twice: once with 10000, once with 5000
        _mockWriteRepository.Verify(x => x.BulkInsertAsync(It.IsAny<IReadOnlyList<ProductSheet>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}

public static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            yield return item;
        }
        await Task.CompletedTask;
    }
}