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

        [Fact]
        public async Task GetTreasuriesAsync_WithExistingTreasuries_ShouldReturnAllTreasuries()
        {
            // Arrange
            var expectedTreasuries = new List<TreasuryEntity>
            {
                CreateTestTreasury(1, "ACC001", "123456", "456789"),
                CreateTestTreasury(2, "ACC002", "234567", "567890"),
                CreateTestTreasury(3, "ACC003", "345678", "678901")
            };
            var cancellationToken = CancellationToken.None;

            _repository.Setup(x => x.GetTreasuriesAsync(cancellationToken))
                .ReturnsAsync(expectedTreasuries.AsReadOnly());

            // Act
            var result = await _repository.Object.GetTreasuriesAsync(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expectedTreasuries);
            _repository.Verify(x => x.GetTreasuriesAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetTreasuriesAsync_WithNoTreasuries_ShouldReturnEmptyCollection()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var expectedEmptyCollection = new List<TreasuryEntity>().AsReadOnly();

            _repository.Setup(x => x.GetTreasuriesAsync(cancellationToken))
                .ReturnsAsync(expectedEmptyCollection);

            // Act
            var result = await _repository.Object.GetTreasuriesAsync(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _repository.Verify(x => x.GetTreasuriesAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetTreasuriesAsync_WithSingleTreasury_ShouldReturnSingleElementCollection()
        {
            // Arrange
            var expectedTreasuries = new List<TreasuryEntity>
            {
                CreateTestTreasury(1, "ACC001", "123456", "456789")
            };
            var cancellationToken = CancellationToken.None;

            _repository.Setup(x => x.GetTreasuriesAsync(cancellationToken))
                .ReturnsAsync(expectedTreasuries.AsReadOnly());

            // Act
            var result = await _repository.Object.GetTreasuriesAsync(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().PortfolioId.Should().Be(1);
            result.First().BankAccount.Should().Be("ACC001");
            result.First().DebitAccount.Should().Be("123456");
            result.First().CreditAccount.Should().Be("456789");
            _repository.Verify(x => x.GetTreasuriesAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetTreasuriesAsync_WithCancelledToken_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var cancellationToken = new CancellationToken(canceled: true);

            _repository.Setup(x => x.GetTreasuriesAsync(
                It.Is<CancellationToken>(ct => ct.IsCancellationRequested)))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _repository.Object.GetTreasuriesAsync(cancellationToken));
        }

        [Fact]
        public async Task GetTreasuriesAsync_ShouldReturnIReadOnlyCollection()
        {
            // Arrange
            var expectedTreasuries = new List<TreasuryEntity>
            {
                CreateTestTreasury(1, "ACC001", "123456", "456789"),
                CreateTestTreasury(2, "ACC002", "234567", "567890")
            };
            var cancellationToken = CancellationToken.None;

            _repository.Setup(x => x.GetTreasuriesAsync(cancellationToken))
                .ReturnsAsync(expectedTreasuries.AsReadOnly());

            // Act
            var result = await _repository.Object.GetTreasuriesAsync(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<IReadOnlyCollection<TreasuryEntity>>();
            result.Count.Should().Be(2);
            _repository.Verify(x => x.GetTreasuriesAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetTreasuriesAsync_WithMixedPortfolioIds_ShouldReturnAllTreasuries()
        {
            // Arrange
            var expectedTreasuries = new List<TreasuryEntity>
            {
                CreateTestTreasury(1, "ACC001", "123456", "456789"),
                CreateTestTreasury(1, "ACC002", "234567", "567890"),
                CreateTestTreasury(2, "ACC001", "345678", "678901"),
                CreateTestTreasury(3, "ACC003", "456789", "789012")
            };
            var cancellationToken = CancellationToken.None;

            _repository.Setup(x => x.GetTreasuriesAsync(cancellationToken))
                .ReturnsAsync(expectedTreasuries.AsReadOnly());

            // Act
            var result = await _repository.Object.GetTreasuriesAsync(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(4);

            var portfolio1Treasuries = result.Where(t => t.PortfolioId == 1);
            portfolio1Treasuries.Should().HaveCount(2);

            var portfolio2Treasuries = result.Where(t => t.PortfolioId == 2);
            portfolio2Treasuries.Should().HaveCount(1);

            var portfolio3Treasuries = result.Where(t => t.PortfolioId == 3);
            portfolio3Treasuries.Should().HaveCount(1);

            _repository.Verify(x => x.GetTreasuriesAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetTreasuriesAsync_WithMixedBankAccounts_ShouldReturnAllTreasuries()
        {
            // Arrange
            var expectedTreasuries = new List<TreasuryEntity>
            {
                CreateTestTreasury(1, "ACC001", "123456", "456789"),
                CreateTestTreasury(1, "ACC002", "234567", "567890"),
                CreateTestTreasury(2, "ACC001", "345678", "678901"),
                CreateTestTreasury(2, "ACC003", "456789", "789012"),
                CreateTestTreasury(3, "ACC002", "567890", "890123")
            };
            var cancellationToken = CancellationToken.None;

            _repository.Setup(x => x.GetTreasuriesAsync(cancellationToken))
                .ReturnsAsync(expectedTreasuries.AsReadOnly());

            // Act
            var result = await _repository.Object.GetTreasuriesAsync(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);

            var acc001Treasuries = result.Where(t => t.BankAccount == "ACC001");
            acc001Treasuries.Should().HaveCount(2);

            var acc002Treasuries = result.Where(t => t.BankAccount == "ACC002");
            acc002Treasuries.Should().HaveCount(2);

            var acc003Treasuries = result.Where(t => t.BankAccount == "ACC003");
            acc003Treasuries.Should().HaveCount(1);

            _repository.Verify(x => x.GetTreasuriesAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetTreasuriesAsync_WithDuplicateBankAccountsDifferentPortfolios_ShouldReturnAllTreasuries()
        {
            // Arrange
            var expectedTreasuries = new List<TreasuryEntity>
            {
                CreateTestTreasury(1, "ACC001", "123456", "456789"),
                CreateTestTreasury(2, "ACC001", "234567", "567890"),
                CreateTestTreasury(3, "ACC001", "345678", "678901")
            };
            var cancellationToken = CancellationToken.None;

            _repository.Setup(x => x.GetTreasuriesAsync(cancellationToken))
                .ReturnsAsync(expectedTreasuries.AsReadOnly());

            // Act
            var result = await _repository.Object.GetTreasuriesAsync(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);

            // Todas deberían tener la misma cuenta bancaria pero diferentes portafolios
            result.All(t => t.BankAccount == "ACC001").Should().BeTrue();

            var portfolioIds = result.Select(t => t.PortfolioId).ToList();
            portfolioIds.Should().Contain(1);
            portfolioIds.Should().Contain(2);
            portfolioIds.Should().Contain(3);

            _repository.Verify(x => x.GetTreasuriesAsync(cancellationToken), Times.Once);
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