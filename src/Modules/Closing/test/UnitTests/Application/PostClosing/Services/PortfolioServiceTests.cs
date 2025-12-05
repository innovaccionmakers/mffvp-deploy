
using Closing.Application.Abstractions.External.Products.Portfolios;
using Closing.Application.PostClosing.Services.PortfolioServices;
using Common.SharedKernel.Core.Primitives; 
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using System.Runtime.Serialization;


namespace Closing.test.UnitTests.Application.PostClosing.Services
{
    public sealed class PortfolioServiceTests
    {
        private readonly Mock<IUpdatePortfolioFromClosingRemote> updateRemoteMock;
        private readonly Mock<IGetPortfolioDataRemote> getRemoteMock ;
        private readonly Mock<ILogger<PortfolioService>> loggerMock;

        public PortfolioServiceTests()
        {
            updateRemoteMock = new(MockBehavior.Strict);
            getRemoteMock = new(MockBehavior.Strict);
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

            var sut = new PortfolioService(updateRemoteMock.Object, getRemoteMock.Object, loggerMock.Object);

            // Act
            await sut.UpdateAsync(portfolioId, closingDate, cancellationToken);

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


            var sut = new PortfolioService(updateRemoteMock.Object, getRemoteMock.Object, loggerMock.Object);

            // Act
            await sut.UpdateAsync(portfolioId, closingDate, CancellationToken.None);

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


            var sut = new PortfolioService(updateRemoteMock.Object, getRemoteMock.Object, loggerMock.Object);

            // Act + Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.UpdateAsync(portfolioId, closingDate, CancellationToken.None));

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


            var sut = new PortfolioService(updateRemoteMock.Object, getRemoteMock.Object, loggerMock.Object);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.UpdateAsync(portfolioId, closingDate, CancellationToken.None));

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


            var sut = new PortfolioService(updateRemoteMock.Object, getRemoteMock.Object, loggerMock.Object);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.UpdateAsync(portfolioId, closingDate, CancellationToken.None));

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

            var sut = new PortfolioService(updateRemoteMock.Object, getRemoteMock.Object, loggerMock.Object);

            // Act + Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                sut.UpdateAsync(portfolioId, closingDate, cts.Token));

            updateRemoteMock.Verify(x => x.ExecuteAsync(It.IsAny<UpdatePortfolioFromClosingRemoteRequest>(), cts.Token), Times.Once);
        }

        [Fact]
        public async Task GetAsyncWhenSucceededTrueCallsRemoteAndDoesNotThrow()
        {
            // Arrange
            var portfolioId = 20;
            var cancellationToken = CancellationToken.None;

            var response = CreateGetResponse(succeeded: true);

            getRemoteMock
                .Setup(x => x.GetAsync(
                    It.Is<GetPortfolioClosingDataRemoteRequest>(r => r.PortfolioId == portfolioId),
                    cancellationToken))
                .ReturnsAsync(Success(response));

            var sut = new PortfolioService(updateRemoteMock.Object, getRemoteMock.Object, loggerMock.Object);

            // Act
            await sut.GetAsync(portfolioId, cancellationToken);

            // Assert
            getRemoteMock.Verify(x => x.GetAsync(
                It.Is<GetPortfolioClosingDataRemoteRequest>(r => r.PortfolioId == portfolioId),
                cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task GetAsyncWhenResultFailureThrowsInvalidOperationException()
        {
            // Arrange
            var portfolioId = 21;

            getRemoteMock
                .Setup(x => x.GetAsync(
                    It.IsAny<GetPortfolioClosingDataRemoteRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Failure<GetPortfolioClosingDataRemoteResponse>(
                    new Error("PRD-GET-001", "Remote error", ErrorType.Failure)));

            var sut = new PortfolioService(updateRemoteMock.Object, getRemoteMock.Object, loggerMock.Object);

            // Act + Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.GetAsync(portfolioId, CancellationToken.None));

            Assert.Contains("PRD-GET-001", ex.Message);
            getRemoteMock.Verify(x => x.GetAsync(
                It.IsAny<GetPortfolioClosingDataRemoteRequest>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAsyncWhenSucceededFalseThrowsInvalidOperationException()
        {
            // Arrange
            var portfolioId = 22;
            var cancellationToken = CancellationToken.None;

            var response = CreateGetResponse(succeeded: false);

            getRemoteMock
                .Setup(x => x.GetAsync(
                    It.IsAny<GetPortfolioClosingDataRemoteRequest>(),
                    cancellationToken))
                .ReturnsAsync(Success(response));

            var sut = new PortfolioService(updateRemoteMock.Object, getRemoteMock.Object, loggerMock.Object);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.GetAsync(portfolioId, cancellationToken));

            getRemoteMock.Verify(x => x.GetAsync(
                It.IsAny<GetPortfolioClosingDataRemoteRequest>(),
                cancellationToken),
                Times.Once);
        }
        [Fact]
        public async Task GetAsyncHonorsCancellationToken()
        {
            // Arrange
            var portfolioId = 23;
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            getRemoteMock
                .Setup(x => x.GetAsync(
                    It.IsAny<GetPortfolioClosingDataRemoteRequest>(),
                    cts.Token))
                .ThrowsAsync(new OperationCanceledException());

            var sut = new PortfolioService(updateRemoteMock.Object, getRemoteMock.Object, loggerMock.Object);

            // Act + Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                sut.GetAsync(portfolioId, cts.Token));

            getRemoteMock.Verify(x => x.GetAsync(
                It.IsAny<GetPortfolioClosingDataRemoteRequest>(),
                cts.Token),
                Times.Once);
        }

        private static GetPortfolioClosingDataRemoteResponse CreateGetResponse(bool succeeded)
        {
            return new GetPortfolioClosingDataRemoteResponse(
                succeeded,
                AgileWithdrawalPercentageProtectedBalance: 10, 
                Code: succeeded ? null : "ERROR",
                Message: succeeded ? "OK" : "Failed"
            );
        }


        private static Result<T> Success<T>(T value) => Result.Success(value);
        private static Result<T> Failure<T>(Error error) => Result.Failure<T>(error);

    }
}
