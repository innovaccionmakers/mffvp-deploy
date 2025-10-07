
using Closing.Application.Abstractions.External.Operations.ClientOperations;
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Application.PostClosing.Services.PendingTransactions;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain; 
using Microsoft.Extensions.Logging;
using Moq;

namespace Closing.test.UnitTests.Application.PostClosing.Services
{
    public sealed class PendingTransactionsServiceTests
    {
        private readonly Mock<IProcessPendingTransactionsRemote> processPendingTransactionsRemoteMock;
        private readonly Mock<ITimeControlService> timeControlServiceMock;
        private readonly Mock<ILogger<PendingTransactionsService>> loggerMock;

        public PendingTransactionsServiceTests()
        {
            processPendingTransactionsRemoteMock = new(MockBehavior.Strict);
            timeControlServiceMock = new(MockBehavior.Strict);
            loggerMock = new(MockBehavior.Loose); 
        }

        [Fact]
        public async Task HandleAsyncWhenRemoteReturnsProcessedCallsEndAsync()
        {
            // Arrange
            var portfolioId = 2;
            var processDateUtc = new DateTime(2025, 10, 07, 0, 0, 0, DateTimeKind.Utc);
            var cancellationToken = CancellationToken.None;
            var expectedIdemKey = $"pendingtx:{portfolioId}:{processDateUtc:yyyyMMdd}";

            var response = new ProcessPendingTransactionsRemoteResponse(
                Succeeded: true,
                Status: "Processed",
                Message: "OK",
                AppliedCount: 10,
                SkippedCount: 0
            );

            processPendingTransactionsRemoteMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<ProcessPendingTransactionsRemoteRequest>(r =>
                        r.PortfolioId == portfolioId &&
                        r.ProcessDateUtc == processDateUtc &&
                        r.IdempotencyKey == expectedIdemKey),
                    cancellationToken))
                .ReturnsAsync(MakeSuccess(response));

            timeControlServiceMock
                .Setup(x => x.EndAsync(portfolioId, cancellationToken))
                .Returns(Task.CompletedTask);

            var sut = new PendingTransactionsService(
                processPendingTransactionsRemoteMock.Object,
                timeControlServiceMock.Object,
                loggerMock.Object);

            // Act
            await sut.HandleAsync(portfolioId, processDateUtc, cancellationToken);

            // Assert
            processPendingTransactionsRemoteMock.VerifyAll();
            timeControlServiceMock.Verify(x => x.EndAsync(portfolioId, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task HandleAsyncWhenRemoteReturnsNothingToProcessCallsEndAsync()
        {
            // Arrange
            var portfolioId = 2;
            var processDateUtc = new DateTime(2025, 10, 07, 0, 0, 0, DateTimeKind.Utc);
            var cancellationToken = CancellationToken.None;

            var response = new ProcessPendingTransactionsRemoteResponse(
                Succeeded: true,
                Status: "NothingToProcess",
                Message: "No items",
                AppliedCount: 0,
                SkippedCount: 0
            );

            processPendingTransactionsRemoteMock
                .Setup(x => x.ExecuteAsync(It.IsAny<ProcessPendingTransactionsRemoteRequest>(), cancellationToken))
                .ReturnsAsync(MakeSuccess(response));

            timeControlServiceMock
                .Setup(x => x.EndAsync(portfolioId, cancellationToken))
                .Returns(Task.CompletedTask);

            var sut = new PendingTransactionsService(
                processPendingTransactionsRemoteMock.Object,
                timeControlServiceMock.Object,
                loggerMock.Object);

            // Act
            await sut.HandleAsync(portfolioId, processDateUtc, cancellationToken);

            // Assert
            timeControlServiceMock.Verify(x => x.EndAsync(portfolioId, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task HandleAsyncWhenRemoteFailureThrowsAndDoesNotCallEndAsync()
        {
            // Arrange
            var portfolioId = 3;
            var processDateUtc = new DateTime(2025, 10, 07, 0, 0, 0, DateTimeKind.Utc);
            var cancellationToken = CancellationToken.None;

            var error = MakeError("OPS-001", "Remote failed");

            processPendingTransactionsRemoteMock
                .Setup(x => x.ExecuteAsync(It.IsAny<ProcessPendingTransactionsRemoteRequest>(), cancellationToken))
                .ReturnsAsync(MakeFailure(error));

            var sut = new PendingTransactionsService(
                processPendingTransactionsRemoteMock.Object,
                timeControlServiceMock.Object, 
                loggerMock.Object);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.HandleAsync(portfolioId, processDateUtc, cancellationToken));

            timeControlServiceMock.Verify(x => x.EndAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsyncWhenRemoteSuccessWithUnexpectedStatusThrowsAndDoesNotCallEndAsync()
        {
            // Arrange
            var portfolioId = 4;
            var processDateUtc = new DateTime(2025, 10, 07, 0, 0, 0, DateTimeKind.Utc);
            var cancellationToken = CancellationToken.None;

            var response = new ProcessPendingTransactionsRemoteResponse(
                Succeeded: true,
                Status: "Partial", 
                Message: "Some pending items remain",
                AppliedCount: 5,
                SkippedCount: 1
            );

            processPendingTransactionsRemoteMock
                .Setup(x => x.ExecuteAsync(It.IsAny<ProcessPendingTransactionsRemoteRequest>(), cancellationToken))
                .ReturnsAsync(MakeSuccess(response));

            var sut = new PendingTransactionsService(
                processPendingTransactionsRemoteMock.Object,
                timeControlServiceMock.Object,
                loggerMock.Object);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.HandleAsync(portfolioId, processDateUtc, cancellationToken));

            timeControlServiceMock.Verify(x => x.EndAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsyncWhenCancellationRequestedThrowsOperationCanceledException()
        {
            // Arrange
            var portfolioId = 5;
            var processDateUtc = new DateTime(2025, 10, 07, 0, 0, 0, DateTimeKind.Utc);
            using var cts = new CancellationTokenSource();
            cts.Cancel(); 

            var sut = new PendingTransactionsService(
                processPendingTransactionsRemoteMock.Object,
                timeControlServiceMock.Object,
                loggerMock.Object);

            // Act + Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                sut.HandleAsync(portfolioId, processDateUtc, cts.Token));

            processPendingTransactionsRemoteMock.Verify(
                x => x.ExecuteAsync(It.IsAny<ProcessPendingTransactionsRemoteRequest>(), It.IsAny<CancellationToken>()),
                Times.Never);

            timeControlServiceMock.Verify(
                x => x.EndAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        private static Result<ProcessPendingTransactionsRemoteResponse> MakeSuccess(ProcessPendingTransactionsRemoteResponse response)
        {
            return Result.Success(response);
        }

        private static Result<ProcessPendingTransactionsRemoteResponse> MakeFailure(Error error)
        {
            return Result.Failure<ProcessPendingTransactionsRemoteResponse>(error);
        }

        private static Error MakeError(string code, string description)
        {
            return new Error(code, description, ErrorType.Validation);

        }
    }
}
