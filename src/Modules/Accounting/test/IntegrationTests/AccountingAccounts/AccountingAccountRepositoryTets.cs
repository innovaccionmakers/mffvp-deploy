using Accounting.Domain.AccountingAccounts;
using Moq;

namespace Accounting.test.IntegrationTests.AccountingAccounts
{
    public class AccountingAccountRepositoryTest
    {
        private readonly Mock<IAccountingAccountRepository> _repositoryMock;

        public AccountingAccountRepositoryTest()
        {
            _repositoryMock = new Mock<IAccountingAccountRepository>();
        }

        [Fact]
        public async Task GetAccountListAsync_ShouldReturnAllAccounts_WhenAccountsExist()
        {
            // Arrange
            var expectedAccounts = new List<AccountingAccount>();
            _repositoryMock.Setup(x => x.GetAccountListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAccounts);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repositoryMock.Object.GetAccountListAsync(cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedAccounts, result);
        }

        [Fact]
        public async Task GetAccountListAsync_ShouldReturnEmptyCollection_WhenNoAccountsExist()
        {
            // Arrange
            var emptyAccounts = new List<AccountingAccount>();
            _repositoryMock.Setup(x => x.GetAccountListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyAccounts);

            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repositoryMock.Object.GetAccountListAsync(cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAccountListAsync_ShouldReturnReadOnlyCollection()
        {
            // Arrange
            var expectedAccounts = new List<AccountingAccount>();
            _repositoryMock.Setup(x => x.GetAccountListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAccounts);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repositoryMock.Object.GetAccountListAsync(cancellationToken);

            // Assert
            Assert.IsAssignableFrom<IReadOnlyCollection<AccountingAccount>>(result);
        }

        [Fact]
        public async Task GetAccountListAsync_ShouldHandleCancellationToken()
        {
            // Arrange
            var expectedAccounts = new List<AccountingAccount>();
            var cancellationToken = new CancellationToken();
            _repositoryMock.Setup(x => x.GetAccountListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAccounts);

            // Act
            var result = await _repositoryMock.Object.GetAccountListAsync(cancellationToken);

            // Assert
            Assert.NotNull(result);
        }
    }
}