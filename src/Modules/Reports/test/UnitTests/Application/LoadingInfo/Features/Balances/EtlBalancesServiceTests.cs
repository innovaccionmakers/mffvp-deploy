
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Reports.Application.LoadingInfo.Features.Balances;
using Reports.Domain.LoadingInfo.Balances;

namespace Reports.test.UnitTests.Application.LoadingInfo.Features.Balances;

public sealed class EtlBalancesServiceTests
{
    private readonly Mock<IBalanceSheetReadRepository> _mockReadRepository;
    private readonly Mock<IBalanceSheetWriteRepository> _mockWriteRepository;
    private readonly Mock<ILogger<EtlBalancesService>> _mockLogger;
    private readonly EtlBalancesService _sut;

    public EtlBalancesServiceTests()
    {
        _mockReadRepository = new Mock<IBalanceSheetReadRepository>();
        _mockWriteRepository = new Mock<IBalanceSheetWriteRepository>();
        _mockLogger = new Mock<ILogger<EtlBalancesService>>();

        _sut = new EtlBalancesService(
         _mockReadRepository.Object,
        _mockWriteRepository.Object,
        _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_ShouldDeleteAndInsertBalances()
    {
        // Arrange
        var closingDate = new DateTime(2024, 1, 31);
        const int portfolioId = 1;

        var sourceData = new List<BalanceSheetReadRow>
        {
            new(
            AffiliateId: 1001,
             PortfolioId: portfolioId,
             GoalId: 1,
             Balance: 1000000m,
             MinimumWages: 2.5m,
             FundId: 100,
             GoalCreatedAtUtc: new DateTime(2023, 1, 1),
            Age: 35,
            IsDependent: false,
            PortfolioEntries: 500000m,
             PortfolioWithdrawals: 100000m,
             ClosingDateUtc: closingDate),
            new(
                 AffiliateId: 1002,
                PortfolioId: portfolioId,
                GoalId: 2,
                Balance: 2000000m,
                MinimumWages: 5.0m,
                FundId: 101,
                GoalCreatedAtUtc: new DateTime(2023, 6, 1),
                Age: 42,
                 IsDependent: false,
                PortfolioEntries: 1000000m,
                 PortfolioWithdrawals: 200000m,
                ClosingDateUtc: closingDate)
        };

        _mockReadRepository
                  .Setup(x => x.ReadBalancesAsync(closingDate, portfolioId, It.IsAny<CancellationToken>()))
                   .Returns(sourceData.ToAsyncEnumerable());

        _mockWriteRepository
         .Setup(x => x.DeleteByClosingDateAndPortfolioAsync(closingDate, portfolioId, It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

        _mockWriteRepository
         .Setup(x => x.BulkInsertAsync(It.IsAny<IReadOnlyList<BalanceSheet>>(), It.IsAny<CancellationToken>()))
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
            x => x.BulkInsertAsync(It.Is<IReadOnlyList<BalanceSheet>>(list => list.Count == 2), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NoDataToProcess_ShouldReturnZeroMetrics()
    {
        // Arrange
        var closingDate = new DateTime(2024, 1, 31);
        const int portfolioId = 1;

        var sourceData = new List<BalanceSheetReadRow>();

        _mockReadRepository
            .Setup(x => x.ReadBalancesAsync(closingDate, portfolioId, It.IsAny<CancellationToken>()))
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
        x => x.BulkInsertAsync(It.IsAny<IReadOnlyList<BalanceSheet>>(), It.IsAny<CancellationToken>()),
        Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_LargeDataSet_ShouldProcessInBatches()
    {
        // Arrange
        var closingDate = new DateTime(2024, 1, 31);
        const int portfolioId = 1;

        // Create 15000 records to ensure multiple batches
        var sourceData = Enumerable.Range(1, 15000)
            .Select(i => new BalanceSheetReadRow(
             AffiliateId: i,
             PortfolioId: portfolioId,
             GoalId: i,
             Balance: 1000000m * i,
             MinimumWages: 2.5m,
            FundId: 100 + i,
             GoalCreatedAtUtc: new DateTime(2023, 1, 1),
            Age: 30 + (i % 30),
            IsDependent: i % 2 == 0,
            PortfolioEntries: 500000m * i,
            PortfolioWithdrawals: 100000m * i,
            ClosingDateUtc: closingDate))
            .ToList();

        _mockReadRepository
         .Setup(x => x.ReadBalancesAsync(closingDate, portfolioId, It.IsAny<CancellationToken>()))
          .Returns(sourceData.ToAsyncEnumerable());

        _mockWriteRepository
              .Setup(x => x.DeleteByClosingDateAndPortfolioAsync(closingDate, portfolioId, It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        _mockWriteRepository
                .Setup(x => x.BulkInsertAsync(It.IsAny<IReadOnlyList<BalanceSheet>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(closingDate, portfolioId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ReadRows.Should().Be(15000);
        result.Value.InsertedRows.Should().Be(15000);

        _mockWriteRepository.Verify(
                    x => x.BulkInsertAsync(It.IsAny<IReadOnlyList<BalanceSheet>>(), It.IsAny<CancellationToken>()),
                    Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var closingDate = new DateTime(2024, 1, 31);
        const int portfolioId = 1;

        _mockReadRepository
        .Setup(x => x.ReadBalancesAsync(closingDate, portfolioId, It.IsAny<CancellationToken>()))
        .Throws(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _sut.ExecuteAsync(closingDate, portfolioId, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ETL_SALDOS_001");
        result.Error.Description.Should().Contain("Database connection failed");
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
        .Setup(x => x.ReadBalancesAsync(closingDate, portfolioId, It.IsAny<CancellationToken>()))
         .Throws(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _sut.ExecuteAsync(closingDate, portfolioId, cts.Token));
    }
}

public static class BalancesAsyncEnumerableExtensions
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