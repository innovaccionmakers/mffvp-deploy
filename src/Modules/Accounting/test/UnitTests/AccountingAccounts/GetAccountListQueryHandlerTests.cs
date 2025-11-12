using Accounting.Application.AccountingAccounts.GetAccountList;
using Accounting.Domain.AccountingAccounts;
using Accounting.Integrations.AccountingAccount.GetAccountList;
using FluentAssertions;
using Moq;

namespace Accounting.test.UnitTests.AccountingAccounts.GetAccountList
{
    public class GetAccountListQueryHandlerTests
    {
        private readonly Mock<IAccountingAccountRepository> _repositoryMock;
        private readonly GetAccountListQueryHandler _handler;
        private readonly CancellationToken _cancellationToken;

        public GetAccountListQueryHandlerTests()
        {
            _repositoryMock = new Mock<IAccountingAccountRepository>();
            _handler = new GetAccountListQueryHandler(_repositoryMock.Object);
            _cancellationToken = CancellationToken.None;
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrowsException_ShouldThrowArgumentNullException()
        {
            // Arrange
            var query = new GetAccountListQuery();
            var expectedException = new Exception("Database connection failed");

            _repositoryMock
                .Setup(repo => repo.GetAccountListAsync(_cancellationToken))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                () => _handler.Handle(query, _cancellationToken));

            exception.Message.Should().Contain("Database connection failed");

            _repositoryMock.Verify(repo => repo.GetAccountListAsync(_cancellationToken), Times.Once);
        }

    }
}