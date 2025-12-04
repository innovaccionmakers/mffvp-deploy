using Accounting.Domain.Treasuries;
using FluentAssertions;
using Moq;
using Products.Domain.Portfolios;
using Treasury.Domain.BankAccounts;
using TreasuryEntity = Accounting.Domain.Treasuries.Treasury;

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
            var expectedEmptyCollection = Enumerable.Empty<TreasuryEntity>();

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
            var expectedEmptyCollection = Enumerable.Empty<TreasuryEntity>();

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
            var accountNumber = new List<string> { "2806052369", "6846052685" };
            var expectedEmptyCollection = Enumerable.Empty<TreasuryEntity>();

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
            var expectedEmptyCollection = Enumerable.Empty<TreasuryEntity>();

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
            var expectedEmptyCollection = Enumerable.Empty<TreasuryEntity>();

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
            var expectedEmptyCollection = Enumerable.Empty<TreasuryEntity>();

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
        [Fact]
        public async Task GetTreasuryAsync_WithValidParameters_ShouldReturnTreasury()
        {
            // Arrange
            var portfolioId = 1;
            var bankAccount = "ACC001";
            var expectedTreasury = CreateTestTreasury(portfolioId, bankAccount, "123456", "456789");
            var cancellationToken = CancellationToken.None;

            _repository.Setup(x => x.GetTreasuryAsync(portfolioId, bankAccount, cancellationToken))
                .ReturnsAsync(expectedTreasury);

            // Act
            var result = await _repository.Object.GetTreasuryAsync(portfolioId, bankAccount, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedTreasury);
            _repository.Verify(x => x.GetTreasuryAsync(portfolioId, bankAccount, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetTreasuryAsync_WithNonExistingTreasury_ShouldReturnNull()
        {
            // Arrange
            var portfolioId = 999;
            var bankAccount = "ACC001";
            var cancellationToken = CancellationToken.None;

            _repository.Setup(x => x.GetTreasuryAsync(portfolioId, bankAccount, cancellationToken))
                .ReturnsAsync((TreasuryEntity)null);

            // Act
            var result = await _repository.Object.GetTreasuryAsync(portfolioId, bankAccount,cancellationToken);

            // Assert
            result.Should().BeNull();
            _repository.Verify(x => x.GetTreasuryAsync(portfolioId, bankAccount, cancellationToken), Times.Once);
        }

        [Fact]
        public void Insert_WithValidTreasury_ShouldCompleteSuccessfully()
        {
            // Arrange
            var treasury = CreateTestTreasury(1, "ACC001", "123456", "456789");

            // Act
            _repository.Object.Insert(treasury);

            // Assert
            _repository.Verify(x => x.Insert(treasury), Times.Once);
        }

        [Fact]
        public void Update_WithValidTreasury_ShouldCompleteSuccessfully()
        {
            // Arrange
            var treasury = CreateTestTreasury(1, "ACC001", "123456", "456789");

            // Act
            _repository.Object.Update(treasury);

            // Assert
            _repository.Verify(x => x.Update(treasury), Times.Once);
        }

        [Fact]
        public void Update_WithModifiedTreasury_ShouldCompleteSuccessfully()
        {
            // Arrange
            var originalTreasury = CreateTestTreasury(1, "ACC001", "123456", "456789");
            var modifiedTreasury = CreateTestTreasury(2, "ACC001", "123456", "456789"); // Modified amount

            // Act
            _repository.Object.Update(modifiedTreasury);

            // Assert
            _repository.Verify(x => x.Update(modifiedTreasury), Times.Once);
        }

        [Fact]
        public void Delete_WithValidTreasury_ShouldCompleteSuccessfully()
        {
            // Arrange
            var treasury = CreateTestTreasury(1, "ACC001", "123456", "456789");

            // Act
            _repository.Object.Delete(treasury);

            // Assert
            _repository.Verify(x => x.Delete(treasury), Times.Once);
        }

        [Fact]
        public void Delete_WithNewlyCreatedTreasury_ShouldCompleteSuccessfully()
        {
            // Arrange
            var treasury = CreateTestTreasury(1, "ACC001", "123456", "456789");

            // Act
            _repository.Object.Delete(treasury);

            // Assert
            _repository.Verify(x => x.Delete(treasury), Times.Once);
        }

        private TreasuryEntity CreateTestTreasury(int portfolioId, string bankAccount, string debitAccount, string creditAccount)
        {
            return Domain.Treasuries.Treasury.Create(
                portfolioId,
                bankAccount,
                debitAccount,
                creditAccount
            ).Value;
        }
    }
}