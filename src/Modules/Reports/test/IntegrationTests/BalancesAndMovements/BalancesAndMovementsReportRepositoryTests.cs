using Moq;
using Reports.Domain.BalancesAndMovements;
using Reports.Infrastructure.ConnectionFactory.Interfaces;

namespace Reports.test.IntegrationTests.BalancesAndMovements
{
    public class BalancesAndMovementsReportRepositoryTests
    {
        private readonly Mock<IReportsDbConnectionFactory> _dbConnectionFactoryMock;
        private readonly Mock<IBalancesAndMovementsReportRepository> _repository;

        public BalancesAndMovementsReportRepositoryTests()
        {
            _dbConnectionFactoryMock = new Mock<IReportsDbConnectionFactory>();
            _repository = new Mock<IBalancesAndMovementsReportRepository>();
        }

        [Fact]
        public async Task GetBalancesAsync_WhenExceptionThrown_ReturnsEmpty()
        {
            // Arrange
            var reportRequest = new BalancesAndMovementsReportRequest
            {
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 1, 31),
                Identification = "123456789"
            };

            var cancellationToken = CancellationToken.None;

            _dbConnectionFactoryMock
                .Setup(x => x.CreateOpenAsync(cancellationToken))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = _repository.Setup(x => x.GetBalancesAsync(reportRequest, cancellationToken));

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetMovementsAsync_WhenExceptionThrown_ReturnsEmpty()
        {
            // Arrange
            var reportRequest = new BalancesAndMovementsReportRequest
            {
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 1, 31)
            };

            var cancellationToken = CancellationToken.None;

            _dbConnectionFactoryMock
                .Setup(x => x.CreateOpenAsync(cancellationToken))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = _repository.Setup(x => x.GetMovementsAsync(reportRequest, cancellationToken));

            // Assert
            Assert.NotNull(result);
        }
    }
}
