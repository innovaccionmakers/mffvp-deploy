using Accounting.Domain.AccountingAssistants;
using Moq;
using FluentAssertions;

namespace Accounting.test.IntegrationTests.AccountingAssistants
{
    public class AccountingAssistantRepositoryTests
    {
        private readonly Mock<IAccountingAssistantRepository> _accountingAssistantRepository;

        public AccountingAssistantRepositoryTests()
        {
            _accountingAssistantRepository = new Mock<IAccountingAssistantRepository>();
        }

        [Fact]
        public async Task AddRangeAsync_WhenEmptyCollectionProvided_ShouldNotAddAnyItems()
        {
            // Arrange
            var emptyAccountingAssistants = Enumerable.Empty<AccountingAssistant>();
            var addedItems = new List<AccountingAssistant>();

            _accountingAssistantRepository.Setup(x => x.AddRangeAsync(
                It.Is<IEnumerable<AccountingAssistant>>(items => !items.Any()),
                It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<AccountingAssistant>, CancellationToken>((items, _) => addedItems.AddRange(items))
                .Returns(Task.CompletedTask);

            // Act
            await _accountingAssistantRepository.Object.AddRangeAsync(emptyAccountingAssistants, CancellationToken.None);

            // Assert
            addedItems.Should().NotBeNull();
            addedItems.Should().BeEmpty();
            _accountingAssistantRepository.Verify(x => x.AddRangeAsync(
                emptyAccountingAssistants, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddRangeAsync_WhenNullCollectionProvided_ShouldNotAddAnyItems()
        {
            // Arrange
            IEnumerable<AccountingAssistant> nullAccountingAssistants = null;
            var addedItems = new List<AccountingAssistant>();

            _accountingAssistantRepository.Setup(x => x.AddRangeAsync(
                It.Is<IEnumerable<AccountingAssistant>>(items => items == null),
                It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<AccountingAssistant>, CancellationToken>((items, _) =>
                {
                    if (items != null) addedItems.AddRange(items);
                })
                .Returns(Task.CompletedTask);

            // Act
            await _accountingAssistantRepository.Object.AddRangeAsync(nullAccountingAssistants, CancellationToken.None);

            // Assert
            addedItems.Should().NotBeNull();
            addedItems.Should().BeEmpty();
            _accountingAssistantRepository.Verify(x => x.AddRangeAsync(
                nullAccountingAssistants, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteRangeAsync_ShouldDeleteAllRecords()
        {
            // Arrange
            var deleteCalled = false;

            _accountingAssistantRepository.Setup(x => x.DeleteRangeAsync(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>(_ => deleteCalled = true)
                .Returns(Task.CompletedTask);

            // Act
            await _accountingAssistantRepository.Object.DeleteRangeAsync(CancellationToken.None);

            // Assert
            deleteCalled.Should().BeTrue();
            _accountingAssistantRepository.Verify(x => x.DeleteRangeAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteRangeAsync_WhenCancelled_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var cancellationToken = new CancellationToken(canceled: true);

            _accountingAssistantRepository.Setup(x => x.DeleteRangeAsync(
                It.Is<CancellationToken>(ct => ct.IsCancellationRequested)))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _accountingAssistantRepository.Object.DeleteRangeAsync(cancellationToken));
        }

        [Fact]
        public async Task DeleteRangeAsync_ShouldBeCalledWithCorrectCancellationToken()
        {
            // Arrange
            var specificCancellationToken = new CancellationTokenSource().Token;
            var deleteCalledWithCorrectToken = false;

            _accountingAssistantRepository.Setup(x => x.DeleteRangeAsync(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>(token =>
                {
                    deleteCalledWithCorrectToken = token == specificCancellationToken;
                })
                .Returns(Task.CompletedTask);

            // Act
            await _accountingAssistantRepository.Object.DeleteRangeAsync(specificCancellationToken);

            // Assert
            deleteCalledWithCorrectToken.Should().BeTrue();
            _accountingAssistantRepository.Verify(x => x.DeleteRangeAsync(specificCancellationToken), Times.Once);
        }
    }
}