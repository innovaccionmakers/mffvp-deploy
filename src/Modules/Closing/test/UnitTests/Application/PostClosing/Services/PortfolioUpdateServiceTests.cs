
using Closing.Application.Abstractions.External.Products.Portfolios;
using Closing.Application.PostClosing.Services.PortfolioUpdate;
using Common.SharedKernel.Core.Primitives; 
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Closing.test.UnitTests.Application.PostClosing.Services
{
    public sealed class PortfolioUpdateServiceTests
    {
        private readonly Mock<IUpdatePortfolioFromClosingRemote> updateRemoteMock;
        private readonly Mock<ILogger<PortfolioUpdateService>> loggerMock;

        public PortfolioUpdateServiceTests()
        {
            updateRemoteMock = new(MockBehavior.Strict);
            loggerMock = new(MockBehavior.Loose); 
        }

        [Fact]
        public async Task ExecuteAsyncWhenUpdatedSucceedsAndLogs()
        {
            // Arrange
            var portfolioId = 12;
            var closingDate = new DateTime(2025, 10, 07, 0, 0, 0, DateTimeKind.Utc);
            var cancellationToken = CancellationToken.None;

            var expectedKey = $"portfolio-upd:{portfolioId}:{closingDate:yyyyMMdd}";
            var response = new UpdatePortfolioFromClosingRemoteResponse(
                Succeeded: true,
                Status: "Updated",
                Message: "OK",
                UpdatedCount: 3
            );

            updateRemoteMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<UpdatePortfolioFromClosingRemoteRequest>(r =>
                        r.PortfolioId == portfolioId &&
                        r.ClosingDateUtc == closingDate &&
                        r.IdempotencyKey == expectedKey &&
                        r.Origin == "Closing"),
                    cancellationToken))
                .ReturnsAsync(Success(response));

            var sut = new PortfolioUpdateService(updateRemoteMock.Object, loggerMock.Object);

            // Act
            await sut.ExecuteAsync(portfolioId, closingDate, cancellationToken);

            // Assert
            updateRemoteMock.VerifyAll();
        }

        [Fact]
        public async Task ExecuteAsyncWhenNoChangeSucceeds()
        {
            // Arrange
            var portfolioId = 13;
            var closingDate = new DateTime(2025, 10, 07, 0, 0, 0, DateTimeKind.Utc);

            updateRemoteMock
                .Setup(x => x.ExecuteAsync(It.IsAny<UpdatePortfolioFromClosingRemoteRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Success(new UpdatePortfolioFromClosingRemoteResponse(true, "NoChange", 0, "No updates")));

            var sut = new PortfolioUpdateService(updateRemoteMock.Object, loggerMock.Object);

            // Act
            await sut.ExecuteAsync(portfolioId, closingDate, CancellationToken.None);

            // Assert
            updateRemoteMock.Verify(x => x.ExecuteAsync(It.IsAny<UpdatePortfolioFromClosingRemoteRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsyncWhenResultFailureThrowsInvalidOperationException()
        {
            // Arrange
            var portfolioId = 14;
            var closingDate = new DateTime(2025, 10, 07, 0, 0, 0, DateTimeKind.Utc);

            updateRemoteMock
                .Setup(x => x.ExecuteAsync(It.IsAny<UpdatePortfolioFromClosingRemoteRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Failure<UpdatePortfolioFromClosingRemoteResponse>(new Error("PRD-001", "Remote error", ErrorType.Failure)));

            var sut = new PortfolioUpdateService(updateRemoteMock.Object, loggerMock.Object);

            // Act + Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.ExecuteAsync(portfolioId, closingDate, CancellationToken.None));

            Assert.Contains("PRD-001", ex.Message);
            updateRemoteMock.Verify(x => x.ExecuteAsync(It.IsAny<UpdatePortfolioFromClosingRemoteRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsyncWhenSucceededFalseThrowsInvalidOperationException()
        {
            // Arrange
            var portfolioId = 15;
            var closingDate = new DateTime(2025, 10, 07, 0, 0, 0, DateTimeKind.Utc);

            var resp = new UpdatePortfolioFromClosingRemoteResponse(
                Succeeded: false,
                Status: "Rejected",
                Message: "Validation failed",
                UpdatedCount: 0);

            updateRemoteMock
                .Setup(x => x.ExecuteAsync(It.IsAny<UpdatePortfolioFromClosingRemoteRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Success(resp));

            var sut = new PortfolioUpdateService(updateRemoteMock.Object, loggerMock.Object);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.ExecuteAsync(portfolioId, closingDate, CancellationToken.None));

            updateRemoteMock.Verify(x => x.ExecuteAsync(It.IsAny<UpdatePortfolioFromClosingRemoteRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsyncWhenUnexpectedStatusThrowsInvalidOperationException()
        {
            // Arrange
            var portfolioId = 16;
            var closingDate = new DateTime(2025, 10, 07, 0, 0, 0, DateTimeKind.Utc);

            var resp = new UpdatePortfolioFromClosingRemoteResponse(
                Succeeded: true,
                Status: "Partial", 
                Message: "Partial update",
                UpdatedCount: 1);

            updateRemoteMock
                .Setup(x => x.ExecuteAsync(It.IsAny<UpdatePortfolioFromClosingRemoteRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Success(resp));

            var sut = new PortfolioUpdateService(updateRemoteMock.Object, loggerMock.Object);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.ExecuteAsync(portfolioId, closingDate, CancellationToken.None));

            updateRemoteMock.Verify(x => x.ExecuteAsync(It.IsAny<UpdatePortfolioFromClosingRemoteRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsyncHonorsCancellationToken()
        {
            // Arrange
            var portfolioId = 17;
            var closingDate = new DateTime(2025, 10, 07, 0, 0, 0, DateTimeKind.Utc);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            updateRemoteMock
                .Setup(x => x.ExecuteAsync(It.IsAny<UpdatePortfolioFromClosingRemoteRequest>(), cts.Token))
                .ThrowsAsync(new OperationCanceledException());

            var sut = new PortfolioUpdateService(updateRemoteMock.Object, loggerMock.Object);

            // Act + Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                sut.ExecuteAsync(portfolioId, closingDate, cts.Token));

            updateRemoteMock.Verify(x => x.ExecuteAsync(It.IsAny<UpdatePortfolioFromClosingRemoteRequest>(), cts.Token), Times.Once);
        }


        private static Result<T> Success<T>(T value) => Result.Success(value);
        private static Result<T> Failure<T>(Error error) => Result.Failure<T>(error);
    }
}
