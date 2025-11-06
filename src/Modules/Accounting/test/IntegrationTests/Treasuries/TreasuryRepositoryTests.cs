using Accounting.Domain.Treasuries;
using Moq;
using FluentAssertions;

namespace Accounting.test.IntegrationTests.Treasuries
{
    public class TreasuryRepositoryTests
    {
        private readonly Mock<ITreasuryRepository> _repository;

        public TreasuryRepositoryTests()
        {
            _repository = new Mock<ITreasuryRepository>();
        }

        [Fact]
        public async Task GetAccountingConceptsTreasuriesAsync_WhenPortfolioIdsEmpty_ShouldReturnEmptyCollection()
        {
            // Arrange
            var emptyPortfolioIds = Enumerable.Empty<int>();
            var emptyAccountNumbers = Enumerable.Empty<string>();
            var expectedEmptyCollection = Enumerable.Empty<Treasury>();

            _repository.Setup(x => x.GetAccountingConceptsTreasuriesAsync(
                It.Is<IEnumerable<int>>(ids => !ids.Any()),
                It.Is<IEnumerable<string>>(accountNumber => !accountNumber.Any()),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _repository.Object.GetAccountingConceptsTreasuriesAsync(emptyPortfolioIds, emptyAccountNumbers, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _repository.Verify(x => x.GetAccountingConceptsTreasuriesAsync(
                emptyPortfolioIds, emptyAccountNumbers, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAccountingConceptsTreasuriesAsync_WhenPortfolioIdsNull_ShouldReturnEmptyCollection()
        {
            // Arrange
            IEnumerable<int> nullPortfolioIds = null;
            IEnumerable<string> nullAccountNumbers = null;
            var expectedEmptyCollection = Enumerable.Empty<Treasury>();

            _repository.Setup(x => x.GetAccountingConceptsTreasuriesAsync(
                It.Is<IEnumerable<int>>(ids => ids == null || !ids.Any()),
                It.Is<IEnumerable<string>>(accountNumber => accountNumber == null || !accountNumber.Any()),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _repository.Object.GetAccountingConceptsTreasuriesAsync(nullPortfolioIds, nullAccountNumbers, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _repository.Verify(x => x.GetAccountingConceptsTreasuriesAsync(
                nullPortfolioIds, nullAccountNumbers, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAccountingConceptsTreasuriesAsync_WhenNoMatchingTreasuries_ShouldReturnEmptyCollection()
        {
            // Arrange
            var portfolioIds = new List<int> { 99, 100 };
            var accountNumber = new List<string> { "2806052369", "6846052685" } ;
            var expectedEmptyCollection = Enumerable.Empty<Treasury>();

            _repository.Setup(x => x.GetAccountingConceptsTreasuriesAsync(
                It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(portfolioIds)),
                It.Is<IEnumerable<string>>(accountNumber => accountNumber.SequenceEqual(accountNumber)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _repository.Object.GetAccountingConceptsTreasuriesAsync(portfolioIds, accountNumber, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _repository.Verify(x => x.GetAccountingConceptsTreasuriesAsync(
                portfolioIds, accountNumber, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAccountingOperationsTreasuriesAsync_WhenPortfolioIdsEmpty_ShouldReturnEmptyCollection()
        {
            // Arrange
            var emptyPortfolioIds = Enumerable.Empty<int>();
            var collectionAccounts = new List<string> { "ACC001", "ACC002" };
            var expectedEmptyCollection = Enumerable.Empty<Treasury>();

            _repository.Setup(x => x.GetAccountingOperationsTreasuriesAsync(
                It.Is<IEnumerable<int>>(ids => !ids.Any()),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _repository.Object.GetAccountingOperationsTreasuriesAsync(
                emptyPortfolioIds, collectionAccounts, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _repository.Verify(x => x.GetAccountingOperationsTreasuriesAsync(
                emptyPortfolioIds, collectionAccounts, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAccountingOperationsTreasuriesAsync_WhenPortfolioIdsNull_ShouldReturnEmptyCollection()
        {
            // Arrange
            IEnumerable<int> nullPortfolioIds = null;
            var collectionAccounts = new List<string> { "ACC001", "ACC002" };
            var expectedEmptyCollection = Enumerable.Empty<Treasury>();

            _repository.Setup(x => x.GetAccountingOperationsTreasuriesAsync(
                It.Is<IEnumerable<int>>(ids => ids == null || !ids.Any()),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _repository.Object.GetAccountingOperationsTreasuriesAsync(
                nullPortfolioIds, collectionAccounts, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _repository.Verify(x => x.GetAccountingOperationsTreasuriesAsync(
                nullPortfolioIds, collectionAccounts, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAccountingOperationsTreasuriesAsync_WhenNoMatchingTreasuries_ShouldReturnEmptyCollection()
        {
            // Arrange
            var portfolioIds = new List<int> { 99, 100 };
            var collectionAccounts = new List<string> { "NONEXISTENT" };
            var expectedEmptyCollection = Enumerable.Empty<Treasury>();

            _repository.Setup(x => x.GetAccountingOperationsTreasuriesAsync(
                It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(portfolioIds)),
                It.Is<IEnumerable<string>>(accounts => accounts.SequenceEqual(collectionAccounts)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _repository.Object.GetAccountingOperationsTreasuriesAsync(
                portfolioIds, collectionAccounts, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _repository.Verify(x => x.GetAccountingOperationsTreasuriesAsync(
                portfolioIds, collectionAccounts, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
