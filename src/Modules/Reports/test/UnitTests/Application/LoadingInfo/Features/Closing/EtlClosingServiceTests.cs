using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Reports.Application.LoadingInfo.Features.Closing;
using Reports.Domain.LoadingInfo.Closing;

namespace Reports.test.UnitTests.Application.LoadingInfo.Features.Closing;

public sealed class EtlClosingServiceTests
{
    private readonly Mock<IClosingSheetReadRepository> _mockReadRepository;
    private readonly Mock<IClosingSheetWriteRepository> _mockWriteRepository;
    private readonly Mock<ILogger<EtlClosingService>> _mockLogger;
    private readonly EtlClosingService _sut;

    public EtlClosingServiceTests()
    {
        _mockReadRepository = new Mock<IClosingSheetReadRepository>();
        _mockWriteRepository = new Mock<IClosingSheetWriteRepository>();
        _mockLogger = new Mock<ILogger<EtlClosingService>>();

        _sut = new EtlClosingService(
           _mockReadRepository.Object,
           _mockWriteRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_ShouldDeleteAndInsertClosingData()
    {
        // Arrange
        var closingDate = new DateTime(2024, 1, 31);
        const int portfolioId = 1;

        var sourceData = new List<ClosingSheetReadRow>
        {
            new()
            {
              PortfolioId = portfolioId,
                ClosingDate = closingDate,
                Contributions = 500000m,
                Withdrawals = 100000m,
                GrossPandL = 50000m,
                Expenses = 5000m,
                DailyFee = 1000m,
                DailyCost = 500m,
                YieldsToCredit = 43500m,
                GrossYieldPerUnit = 0.05m,
                CostPerUnit = 0.01m,
                UnitValue = 1000m,
                Units = 1000m,
                PortfolioValue = 1000000m,
                Participants = 100,
                PortfolioFeePercentage = 0.02m,
                FundId = 100
              },
             new()
             {
                  PortfolioId = portfolioId,
                ClosingDate = closingDate,
                Contributions = 300000m,
                Withdrawals = 50000m,
                GrossPandL = 25000m,
                 Expenses = 2500m,
                 DailyFee = 500m,
                DailyCost = 250m,
                YieldsToCredit = 21750m,
                GrossYieldPerUnit = 0.03m,
                 CostPerUnit = 0.005m,
                UnitValue = 1000m,
                Units = 500m,
                 PortfolioValue = 500000m,
                 Participants = 50,
                PortfolioFeePercentage = 0.015m,
                FundId = 101
            }
        };

        _mockReadRepository
            .Setup(x => x.ReadClosingAsync(closingDate, portfolioId, It.IsAny<CancellationToken>()))
            .Returns(sourceData.ToAsyncEnumerable());

        _mockWriteRepository
             .Setup(x => x.DeleteByClosingDateAndPortfolioAsync(closingDate, portfolioId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockWriteRepository
            .Setup(x => x.BulkInsertAsync(It.IsAny<IReadOnlyList<ClosingSheet>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(closingDate, portfolioId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ReadRows.Should().Be(2);
        result.Value.InsertedRows.Should().Be(2);

        _mockWriteRepository.Verify(
            x => x.DeleteByClosingDateAndPortfolioAsync(closingDate, portfolioId, It.IsAny<CancellationToken>()),
                 Times.Once);
        _mockWriteRepository.Verify(
              x => x.BulkInsertAsync(It.Is<IReadOnlyList<ClosingSheet>>(list => list.Count == 2), It.IsAny<CancellationToken>()),
             Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullFundId_ShouldReturnFailure()
    {
        // Arrange
        var closingDate = new DateTime(2024, 1, 31);
        const int portfolioId = 1;

        var sourceData = new List<ClosingSheetReadRow>
  {
            new()
         {
        PortfolioId = portfolioId,
        ClosingDate = closingDate,
        Contributions = 500000m,
        Withdrawals = 100000m,
         GrossPandL = 50000m,
        Expenses = 5000m,
        DailyFee = 1000m,
        DailyCost = 500m,
        YieldsToCredit = 43500m,
        GrossYieldPerUnit = 0.05m,
        CostPerUnit = 0.01m,
        UnitValue = 1000m,
         Units = 1000m,
        PortfolioValue = 1000000m,
        Participants = 100,
        PortfolioFeePercentage = 0.02m,
       FundId = null // NULL FundId
      }
  };

        _mockReadRepository
         .Setup(x => x.ReadClosingAsync(closingDate, portfolioId, It.IsAny<CancellationToken>()))
          .Returns(sourceData.ToAsyncEnumerable());

        _mockWriteRepository
         .Setup(x => x.DeleteByClosingDateAndPortfolioAsync(closingDate, portfolioId, It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(closingDate, portfolioId, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ETL_CLOSING_NULL_FUND");
        result.Error.Description.Should().Contain("FundId es NULL");
    }

    [Fact]
    public async Task ExecuteAsync_NoDataToProcess_ShouldReturnZeroMetrics()
    {
        // Arrange
        var closingDate = new DateTime(2024, 1, 31);
        const int portfolioId = 1;

        var sourceData = new List<ClosingSheetReadRow>();

        _mockReadRepository
          .Setup(x => x.ReadClosingAsync(closingDate, portfolioId, It.IsAny<CancellationToken>()))
          .Returns(sourceData.ToAsyncEnumerable());

        _mockWriteRepository
            .Setup(x => x.DeleteByClosingDateAndPortfolioAsync(closingDate, portfolioId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(closingDate, portfolioId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ReadRows.Should().Be(0);
        result.Value.InsertedRows.Should().Be(0);

        _mockWriteRepository.Verify(
            x => x.DeleteByClosingDateAndPortfolioAsync(closingDate, portfolioId, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockWriteRepository.Verify(
          x => x.BulkInsertAsync(It.IsAny<IReadOnlyList<ClosingSheet>>(), It.IsAny<CancellationToken>()),
             Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_LargeDataSet_ShouldProcessInBatches()
    {
        // Arrange
        var closingDate = new DateTime(2024, 1, 31);
        const int portfolioId = 1;

        // Create 15000 records
        var sourceData = Enumerable.Range(1, 15000)
        .Select(i => new ClosingSheetReadRow
        {
            PortfolioId = portfolioId,
            ClosingDate = closingDate,
            Contributions = 500000m + i,
            Withdrawals = 100000m + i,
            GrossPandL = 50000m + i,
            Expenses = 5000m + i,
            DailyFee = 1000m + i,
            DailyCost = 500m + i,
            YieldsToCredit = 43500m + i,
            GrossYieldPerUnit = 0.05m,
            CostPerUnit = 0.01m,
            UnitValue = 1000m + i,
            Units = 1000m + i,
            PortfolioValue = 1000000m + i,
            Participants = 100 + i,
            PortfolioFeePercentage = 0.02m,
            FundId = 100 + i
        })
            .ToList();

        _mockReadRepository
            .Setup(x => x.ReadClosingAsync(closingDate, portfolioId, It.IsAny<CancellationToken>()))
            .Returns(sourceData.ToAsyncEnumerable());

        _mockWriteRepository
            .Setup(x => x.DeleteByClosingDateAndPortfolioAsync(closingDate, portfolioId, It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        _mockWriteRepository
         .Setup(x => x.BulkInsertAsync(It.IsAny<IReadOnlyList<ClosingSheet>>(), It.IsAny<CancellationToken>()))
         .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(closingDate, portfolioId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ReadRows.Should().Be(15000);
        result.Value.InsertedRows.Should().Be(15000);

        // Should be called twice: once with 10000, once with 5000
        _mockWriteRepository.Verify(
               x => x.BulkInsertAsync(It.IsAny<IReadOnlyList<ClosingSheet>>(), It.IsAny<CancellationToken>()),
                  Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var closingDate = new DateTime(2024, 1, 31);
        const int portfolioId = 1;

        _mockReadRepository
          .Setup(x => x.ReadClosingAsync(closingDate, portfolioId, It.IsAny<CancellationToken>()))
        .Throws(new InvalidOperationException("Database error"));

        // Act
        var result = await _sut.ExecuteAsync(closingDate, portfolioId, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ETL_CLOSING_001");
        result.Error.Description.Should().Contain("Database error");
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var closingDate = new DateTime(2024, 1, 31);
        const int portfolioId = 1;
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockReadRepository
               .Setup(x => x.ReadClosingAsync(closingDate, portfolioId, It.IsAny<CancellationToken>()))
               .Throws(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
         _sut.ExecuteAsync(closingDate, portfolioId, cts.Token));
    }
}

public static class ClosingAsyncEnumerableExtensions
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