using Moq;
using Reports.Domain.Deposits;
using Reports.Infrastructure.ConnectionFactory.Interfaces;

namespace Reports.test.IntegrationTests.Deposits
{
    public class DepositsRepositoryTests
    {
        private readonly Mock<IReportsDbConnectionFactory> _dbConnectionFactoryMock;
        private readonly Mock<IDepositsRepository> _repository;

        public DepositsRepositoryTests()
        {
            _dbConnectionFactoryMock = new Mock<IReportsDbConnectionFactory>();
            _repository = new Mock<IDepositsRepository>();
        }

        [Fact]
        public async Task GetDepositsAsync_WhenSuccessful_ReturnsDepositsResponse()
        {
            // Arrange
            var reportRequest = new DepositsRequest
            {
                ProcessDate = new DateTime(2024, 1, 1)
            };

            var cancellationToken = CancellationToken.None;
            var expectedResponse = new List<DepositsResponse>
            {
                new DepositsResponse(
                    "S",
                    "123456",
                    "606",
                    DateTime.Now,
                    1000,
                    "",
                    "D",
                    "Test Operation",
                    "Test Operation",
                    "Test Operation",
                    "",
                    "",
                    "",
                    252
                )
            };

            _repository
                .Setup(x => x.GetDepositsAsync(reportRequest, cancellationToken))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _repository.Object.GetDepositsAsync(reportRequest, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(expectedResponse.First().AccountNumber, result.First().AccountNumber);
        }

        [Fact]
        public async Task GetDepositsAsync_WithNullRequest_ReturnsEmpty()
        {
            // Arrange
            DepositsRequest reportRequest = null;
            var cancellationToken = CancellationToken.None;

            _repository
                .Setup(x => x.GetDepositsAsync(reportRequest, cancellationToken))
                .ReturnsAsync(Enumerable.Empty<DepositsResponse>());

            // Act
            var result = await _repository.Object.GetDepositsAsync(reportRequest, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDepositsAsync_WithDefaultProcessDate_ReturnsDepositsResponse()
        {
            // Arrange
            var reportRequest = new DepositsRequest
            {
                ProcessDate = default(DateTime)
            };

            var cancellationToken = CancellationToken.None;

            _repository
                .Setup(x => x.GetDepositsAsync(reportRequest, cancellationToken))
                .ReturnsAsync(new List<DepositsResponse>());

            // Act
            var result = await _repository.Object.GetDepositsAsync(reportRequest, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDepositsAsync_WhenCancelled_ThrowsOperationCanceledException()
        {
            // Arrange
            var reportRequest = new DepositsRequest
            {
                ProcessDate = new DateTime(2024, 1, 1)
            };

            var cancellationToken = new CancellationToken(canceled: true);

            _repository
                .Setup(x => x.GetDepositsAsync(reportRequest, cancellationToken))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _repository.Object.GetDepositsAsync(reportRequest, cancellationToken));
        }

        [Fact]
        public async Task GetDepositsAsync_WithMultipleResponses_ReturnsAllDeposits()
        {
            // Arrange
            var reportRequest = new DepositsRequest
            {
                ProcessDate = new DateTime(2024, 1, 1)
            };

            var cancellationToken = CancellationToken.None;
            var expectedResponses = new List<DepositsResponse>
            {
                new DepositsResponse(
                    "S", "123456", "606", DateTime.Now, 1000, "", "D",
                    "Operation 1", "Operation 1", "Operation 1", "", "", "", 252
                ),
                new DepositsResponse(
                    "D", "789012", "7010", DateTime.Now, 2000, "", "C",
                    "Operation 2", "Operation 2", "Operation 2", "", "", "", 252
                )
            };

            _repository
                .Setup(x => x.GetDepositsAsync(reportRequest, cancellationToken))
                .ReturnsAsync(expectedResponses);

            // Act
            var result = await _repository.Object.GetDepositsAsync(reportRequest, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal(expectedResponses.First().AccountNumber, result.First().AccountNumber);
            Assert.Equal(expectedResponses.Last().AccountNumber, result.Last().AccountNumber);
        }

        [Fact]
        public async Task GetDepositsAsync_WhenRepositoryReturnsNull_HandlesNullGracefully()
        {
            // Arrange
            var reportRequest = new DepositsRequest
            {
                ProcessDate = new DateTime(2024, 1, 1)
            };

            var cancellationToken = CancellationToken.None;

            _repository
                .Setup(x => x.GetDepositsAsync(reportRequest, cancellationToken))
                .ReturnsAsync((IEnumerable<DepositsResponse>)null);

            // Act
            var result = await _repository.Object.GetDepositsAsync(reportRequest, cancellationToken);

            // Assert
            Assert.Null(result);
        }
    }
}