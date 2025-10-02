using Accounting.Domain.PassiveTransactions;
using FluentAssertions;
using Moq;

namespace Accounting.test.IntegrationTests.PassiveTransactions
{
    public class PassiveTransactionRepositoryTests
    {
        private readonly Mock<IPassiveTransactionRepository> _mockPassiveTransactionRepository = new Mock<IPassiveTransactionRepository>();

        public PassiveTransactionRepositoryTests()
        {
            _mockPassiveTransactionRepository = new Mock<IPassiveTransactionRepository>();
        }

        [Fact]
        public async Task GetByPortfolioIdAsync_WhenPassiveTransactionDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var portfolioId = 999;
            var operationTypeId = 888;

            _mockPassiveTransactionRepository.Setup(x => x.GetByPortfolioIdAndOperationTypeAsync(
                It.Is<int>(id => id == portfolioId),
                It.Is<long>(operationTypeId => operationTypeId == operationTypeId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((PassiveTransaction?)null);

            // Act
            var result = await _mockPassiveTransactionRepository.Object.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, CancellationToken.None);

            // Assert
            result.Should().BeNull();
            _mockPassiveTransactionRepository.Verify(x => x.GetByPortfolioIdAndOperationTypeAsync(
                portfolioId, operationTypeId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAccountingOperationsAsync_WhenPortfolioIdsEmpty_ShouldReturnEmptyCollection()
        {
            // Arrange
            var emptyPortfolioIds = Enumerable.Empty<int>();
            var typeOperationsIds = new List<long> { 1001, 1002 };
            var expectedEmptyCollection = Enumerable.Empty<PassiveTransaction?>();

            _mockPassiveTransactionRepository.Setup(x => x.GetAccountingOperationsAsync(
                It.Is<IEnumerable<int>>(ids => !ids.Any()),
                It.IsAny<IEnumerable<long>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _mockPassiveTransactionRepository.Object.GetAccountingOperationsAsync(
                emptyPortfolioIds, typeOperationsIds, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockPassiveTransactionRepository.Verify(x => x.GetAccountingOperationsAsync(
                emptyPortfolioIds, typeOperationsIds, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAccountingOperationsAsync_WhenPortfolioIdsNull_ShouldReturnEmptyCollection()
        {
            // Arrange
            IEnumerable<int> nullPortfolioIds = null;
            var typeOperationsIds = new List<long> { 1001, 1002 };
            var expectedEmptyCollection = Enumerable.Empty<PassiveTransaction?>();

            _mockPassiveTransactionRepository.Setup(x => x.GetAccountingOperationsAsync(
                It.Is<IEnumerable<int>>(ids => ids == null || !ids.Any()),
                It.IsAny<IEnumerable<long>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _mockPassiveTransactionRepository.Object.GetAccountingOperationsAsync(
                nullPortfolioIds, typeOperationsIds, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockPassiveTransactionRepository.Verify(x => x.GetAccountingOperationsAsync(
                nullPortfolioIds, typeOperationsIds, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAccountingOperationsAsync_WhenNoMatchingPassiveTransactions_ShouldReturnEmptyCollection()
        {
            // Arrange
            var portfolioIds = new List<int> { 99, 100 };
            var typeOperationsIds = new List<long> { 9999 };
            var expectedEmptyCollection = Enumerable.Empty<PassiveTransaction?>();

            _mockPassiveTransactionRepository.Setup(x => x.GetAccountingOperationsAsync(
                It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(portfolioIds)),
                It.Is<IEnumerable<long>>(types => types.SequenceEqual(typeOperationsIds)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _mockPassiveTransactionRepository.Object.GetAccountingOperationsAsync(
                portfolioIds, typeOperationsIds, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockPassiveTransactionRepository.Verify(x => x.GetAccountingOperationsAsync(
                portfolioIds, typeOperationsIds, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAccountingOperationsAsync_WhenBothParametersEmpty_ShouldReturnEmptyCollection()
        {
            // Arrange
            var emptyPortfolioIds = Enumerable.Empty<int>();
            var emptyTypeOperationsIds = Enumerable.Empty<long>();
            var expectedEmptyCollection = Enumerable.Empty<PassiveTransaction?>();

            _mockPassiveTransactionRepository.Setup(x => x.GetAccountingOperationsAsync(
                It.Is<IEnumerable<int>>(ids => !ids.Any()),
                It.Is<IEnumerable<long>>(types => !types.Any()),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _mockPassiveTransactionRepository.Object.GetAccountingOperationsAsync(
                emptyPortfolioIds, emptyTypeOperationsIds, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockPassiveTransactionRepository.Verify(x => x.GetAccountingOperationsAsync(
                emptyPortfolioIds, emptyTypeOperationsIds, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAccountingOperationsAsync_WhenCancelled_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var portfolioIds = new List<int> { 1, 2 };
            var typeOperationsIds = new List<long> { 1001, 1002 };
            var cancellationToken = new CancellationToken(canceled: true);

            _mockPassiveTransactionRepository.Setup(x => x.GetAccountingOperationsAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<IEnumerable<long>>(),
                It.Is<CancellationToken>(ct => ct.IsCancellationRequested)))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _mockPassiveTransactionRepository.Object.GetAccountingOperationsAsync(
                    portfolioIds, typeOperationsIds, cancellationToken));
        }

        [Fact]
        public async Task GetByPortfolioIdAsync_WhenCancelled_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var portfolioId = 1;
            var operationTypeId = 888;
            var cancellationToken = new CancellationToken(canceled: true);

            _mockPassiveTransactionRepository.Setup(x => x.GetByPortfolioIdAndOperationTypeAsync(
                It.IsAny<int>(),
                It.IsAny<long>(),
                It.Is<CancellationToken>(ct => ct.IsCancellationRequested)))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _mockPassiveTransactionRepository.Object.GetByPortfolioIdAndOperationTypeAsync(
                    portfolioId, operationTypeId, cancellationToken));
        }
    }
}
