using Common.SharedKernel.Domain;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Reports.Application.LoadingInfo.Features.People;
using Reports.Application.LoadingInfo.Models;
using Reports.Domain.LoadingInfo.Audit;
using Reports.Domain.LoadingInfo.People;

namespace Reports.test.UnitTests.Application.LoadingInfo.Features.People;

public sealed class EtlClientMembershipServiceTests
{
    private readonly Mock<IPeopleSheetReadRepository> _mockReadRepository;
    private readonly Mock<IPeopleSheetWriteRepository> _mockWriteRepository;
    private readonly Mock<IEtlExecutionRepository> _mockExecutionRepository;
    private readonly Mock<ILogger<EtlClientMembershipService>> _mockLogger;
    private readonly EtlClientMembershipService _sut;

    public EtlClientMembershipServiceTests()
    {
        _mockReadRepository = new Mock<IPeopleSheetReadRepository>();
        _mockWriteRepository = new Mock<IPeopleSheetWriteRepository>();
        _mockExecutionRepository = new Mock<IEtlExecutionRepository>();
        _mockLogger = new Mock<ILogger<EtlClientMembershipService>>();

        _sut = new EtlClientMembershipService(
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

        var sourceData = new List<PeopleSheetReadRow>
        {
            new() { IdentificationType = "CC", IdentificationTypeHomologated = "CC", MemberId = 1001, Identification = "123456", FullName = "John Doe", Birthday = new DateTime(1990, 1, 1), Gender = "M" },
            new() { IdentificationType = "CE", IdentificationTypeHomologated = "CE", MemberId = 1002, Identification = "789012", FullName = "Jane Smith", Birthday = new DateTime(1985, 5, 15), Gender = "F" }
        };

        _mockReadRepository
              .Setup(x => x.ReadPeopleAsync(null, It.IsAny<CancellationToken>()))
              .Returns(sourceData.ToAsyncEnumerable());

        _mockWriteRepository
           .Setup(x => x.TruncateAsync(It.IsAny<CancellationToken>()))
           .Returns(Task.CompletedTask);

        _mockWriteRepository
          .Setup(x => x.BulkInsertAsync(It.IsAny<IReadOnlyList<PeopleSheet>>(), It.IsAny<CancellationToken>()))
         .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ReadRows.Should().Be(2);
        result.Value.InsertedRows.Should().Be(2);

        _mockWriteRepository.Verify(x => x.TruncateAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockWriteRepository.Verify(x => x.DeleteByMemberIdsAsync(It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockWriteRepository.Verify(x => x.BulkInsertAsync(It.Is<IReadOnlyList<PeopleSheet>>(list => list.Count == 2), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_IncrementalLoad_ShouldDeleteAndInsertUpdatedData()
    {
        // Arrange
        var lastRowVersion = 9876543210L;
        _mockExecutionRepository
             .Setup(x => x.GetLastSuccessfulExecutionTimestampAsync("loading-info", It.IsAny<CancellationToken>()))
             .ReturnsAsync(lastRowVersion);

        var sourceData = new List<PeopleSheetReadRow>
   {
        new() { IdentificationType = "CC", IdentificationTypeHomologated = "CC", MemberId = 1001, Identification = "123456", FullName = "John Doe Updated", Birthday = new DateTime(1990, 1, 1), Gender = "M" },
        new() { IdentificationType = "TI", IdentificationTypeHomologated = "TI", MemberId = 1003, Identification = "345678", FullName = "New Person", Birthday = new DateTime(2000, 3, 20), Gender = "M" }
        };

        _mockReadRepository
        .Setup(x => x.ReadPeopleAsync(lastRowVersion, It.IsAny<CancellationToken>()))
         .Returns(sourceData.ToAsyncEnumerable());

        _mockWriteRepository
         .Setup(x => x.DeleteByMemberIdsAsync(It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<CancellationToken>()))
           .Returns(Task.CompletedTask);

        _mockWriteRepository
          .Setup(x => x.BulkInsertAsync(It.IsAny<IReadOnlyList<PeopleSheet>>(), It.IsAny<CancellationToken>()))
           .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ReadRows.Should().Be(2);
        result.Value.InsertedRows.Should().Be(2);

        _mockWriteRepository.Verify(x => x.TruncateAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockWriteRepository.Verify(x => x.DeleteByMemberIdsAsync(
          It.Is<IReadOnlyCollection<long>>(ids => ids.Contains(1001L) && ids.Contains(1003L) && ids.Count == 2),
        It.IsAny<CancellationToken>()), Times.Once);
        _mockWriteRepository.Verify(x => x.BulkInsertAsync(It.Is<IReadOnlyList<PeopleSheet>>(list => list.Count == 2), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NoDataToProcess_ShouldReturnZeroMetrics()
    {
        // Arrange
        _mockExecutionRepository
        .Setup(x => x.GetLastSuccessfulExecutionTimestampAsync("loading-info", It.IsAny<CancellationToken>()))
        .ReturnsAsync((long?)null);

        var sourceData = new List<PeopleSheetReadRow>();

        _mockReadRepository
          .Setup(x => x.ReadPeopleAsync(null, It.IsAny<CancellationToken>()))
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
        _mockWriteRepository.Verify(x => x.BulkInsertAsync(It.IsAny<IReadOnlyList<PeopleSheet>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        _mockExecutionRepository
    .Setup(x => x.GetLastSuccessfulExecutionTimestampAsync("loading-info", It.IsAny<CancellationToken>()))
    .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ETL_PEOPLE_001");
        result.Error.Description.Should().Contain("Database connection failed");
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

        // Create 15000 records to ensure multiple batches
        var sourceData = Enumerable.Range(1, 15000)
       .Select(i => new PeopleSheetReadRow
       {
           IdentificationType = "CC",
           IdentificationTypeHomologated = "CC",
           MemberId = i,
           Identification = $"ID{i}",
           FullName = $"Person {i}",
           Birthday = new DateTime(1980 + (i % 40), 1, 1),
           Gender = i % 2 == 0 ? "M" : "F"
       })
       .ToList();

        _mockReadRepository
        .Setup(x => x.ReadPeopleAsync(null, It.IsAny<CancellationToken>()))
            .Returns(sourceData.ToAsyncEnumerable());

        _mockWriteRepository
            .Setup(x => x.TruncateAsync(It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

        _mockWriteRepository
          .Setup(x => x.BulkInsertAsync(It.IsAny<IReadOnlyList<PeopleSheet>>(), It.IsAny<CancellationToken>()))
         .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ReadRows.Should().Be(15000);
        result.Value.InsertedRows.Should().Be(15000);

        // Should be called twice: once with 10000, once with 5000
        _mockWriteRepository.Verify(x => x.BulkInsertAsync(It.IsAny<IReadOnlyList<PeopleSheet>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
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