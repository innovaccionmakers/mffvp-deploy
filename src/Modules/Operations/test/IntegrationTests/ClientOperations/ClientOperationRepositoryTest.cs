using Moq;
using Operations.Domain.ClientOperations;

namespace Operations.test.IntegrationTests.ClientOperations
{
    public class ClientOperationRepositoryTest
    {
        private readonly Mock<IClientOperationRepository> _repositoryMock;

        public ClientOperationRepositoryTest()
        {
            _repositoryMock = new Mock<IClientOperationRepository>();
        }

        [Fact]
        public async Task GetClientOperationsByProcessDateAsync_WithNoMatchingOperations_ReturnsEmptyList()
        {
            // Arrange
            var processDate = new DateTime(2025, 1, 16);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repositoryMock.Object.GetClientOperationsByProcessDateAsync(processDate, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}