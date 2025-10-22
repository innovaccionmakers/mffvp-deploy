using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage; 
using Microsoft.Extensions.Logging;
using Moq;
using Operations.Application.Abstractions.Data;
using Operations.Application.TrustOperations.Commands;
using Operations.Domain.TrustOperations;
using Operations.Integrations.TrustOperations.Commands;

namespace Operations.test.UnitTests.Application.TrustOperations.Commands
{
    public class UpsertTrustOpFromClosingCommandHandlerTests
    {
        private readonly Mock<ITrustOperationBulkRepository> repository = new();
        private readonly Mock<IUnitOfWork> unitOfWork = new();
        private readonly Mock<ILogger<UpsertTrustOpFromClosingCommandHandler>> logger = new();

   
        private static Mock<IDbContextTransaction> CreateTransactionMock()
        {
            var tx = new Mock<IDbContextTransaction>();
            tx.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            tx.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            tx.Setup(t => t.Dispose()).Callback(() => { });
            tx.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);
            return tx;
        }

        private UpsertTrustOpFromClosingCommandHandler CreateSut(Mock<IDbContextTransaction> txMock)
        {
            unitOfWork
                .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(txMock.Object);

            return new UpsertTrustOpFromClosingCommandHandler(
                repository.Object, unitOfWork.Object, logger.Object);
        }

        [Fact]
        public async Task ReturnsSuccessAndCommitsOnHappyPath()
        {
            // Arrange
            var tx = CreateTransactionMock();
            var sut = CreateSut(tx);

            var now = DateTime.UtcNow.Date;
            var items = new List<TrustYieldOperationFromClosing>
            {
                new(TrustId: 11, OperationTypeId: 101, Amount: 123.45m, ClientOperationId: 9001, ProcessDateUtc: now),
                new(TrustId: 22, OperationTypeId: 202, Amount: 678.90m, ClientOperationId: 9002, ProcessDateUtc: now)
            };

            var cmd = new UpsertTrustOpFromClosingCommand(
                PortfolioId: 7,
                TrustYieldOperations: items);

            var bulkResult = new UpsertBulkResult(
                Inserted: 2,
                Updated: 0,
                ChangedTrustIds: new long[] { 11, 22 });

            repository
                .Setup(r => r.UpsertBulkAsync(
                    It.IsAny<int>(),
                    It.IsAny<IReadOnlyList<TrustYieldOpRowForBulk>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(bulkResult);

            var cancellationToken = new CancellationTokenSource().Token;

            // Act
            var result = await sut.Handle(cmd, cancellationToken);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Inserted.Should().Be(2);
            result.Value.Updated.Should().Be(0);
            result.Value.ChangedTrustIds.Should().BeEquivalentTo(new long[] { 11, 22 });

            repository.Verify(r => r.UpsertBulkAsync(
                7,
                It.Is<IReadOnlyList<TrustYieldOpRowForBulk>>(rows =>
                    rows.Count == 2 &&
                    rows[0].TrustId == 11 &&
                    rows[0].OperationTypeId == 101 &&
                    rows[0].Amount == 123.45m &&
                    rows[0].ClientOperationId == 9001 &&
                    rows[0].ProcessDateUtc == now &&
                    rows[1].TrustId == 22 &&
                    rows[1].OperationTypeId == 202 &&
                    rows[1].Amount == 678.90m &&
                    rows[1].ClientOperationId == 9002 &&
                    rows[1].ProcessDateUtc == now
                ),
                cancellationToken),
                Times.Once);

            unitOfWork.Verify(u => u.BeginTransactionAsync(cancellationToken), Times.Once);
            tx.Verify(t => t.CommitAsync(cancellationToken), Times.Once);
            tx.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ThrowsOperationCanceledAndRollsBack()
        {
            // Arrange
            var tx = CreateTransactionMock();
            var sut = CreateSut(tx);

            var items = new List<TrustYieldOperationFromClosing>
            {
                new(TrustId: 1, OperationTypeId: 1, Amount: 1m, ClientOperationId: 1, ProcessDateUtc: DateTime.UtcNow.Date)
            };

            var cmd = new UpsertTrustOpFromClosingCommand(PortfolioId: 3, TrustYieldOperations: items);

            repository
                .Setup(r => r.UpsertBulkAsync(
                    It.IsAny<int>(),
                    It.IsAny<IReadOnlyList<TrustYieldOpRowForBulk>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var cancellationToken = new CancellationTokenSource().Token;

            // Act
            Func<Task> act = async () => await sut.Handle(cmd, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();

            tx.Verify(t => t.RollbackAsync(CancellationToken.None), Times.Once);
            tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ReturnsFailureAndLogsAndRollsBackOnException()
        {
            // Arrange
            var tx = CreateTransactionMock();
            var sut = CreateSut(tx);

            var items = new List<TrustYieldOperationFromClosing>
            {
                new(TrustId: 5, OperationTypeId: 50, Amount: 10m, ClientOperationId: 77, ProcessDateUtc: DateTime.UtcNow.Date)
            };

            var cmd = new UpsertTrustOpFromClosingCommand(PortfolioId: 9, TrustYieldOperations: items);

            repository
                .Setup(r => r.UpsertBulkAsync(
                    It.IsAny<int>(),
                    It.IsAny<IReadOnlyList<TrustYieldOpRowForBulk>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("db error"));

            var cancellationToken = new CancellationTokenSource().Token;

            // Act
            var result = await sut.Handle(cmd, cancellationToken);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("OPS-UPSERT-ERR");
            result.Error.Description.Should().Contain("db error");

            tx.Verify(t => t.RollbackAsync(CancellationToken.None), Times.Once);
            tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);

            logger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Error en upsert bulk de operaciones desde Closing")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
