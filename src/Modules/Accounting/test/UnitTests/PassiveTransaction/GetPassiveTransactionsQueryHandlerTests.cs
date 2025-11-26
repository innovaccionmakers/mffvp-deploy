using Accounting.Application.PassiveTransaction.GetPassiveTransactions;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.PassiveTransaction.GetPassiveTransactions;
using Common.SharedKernel.Application.Messaging;
using Microsoft.Extensions.Logging;
using Moq;

namespace Accounting.test.UnitTests.PassiveTransaction
{
    public class GetPassiveTransactionsQueryHandlerTests
    {
        private readonly Mock<IPassiveTransactionRepository> _mockRepository;
        private readonly Mock<ILogger<GetPassiveTransactionsQueryHandler>> _mockLogger;
        private readonly IQueryHandler<GetPassiveTransactionsQuery, GetPassiveTransactionsResponse> _handler;

        public GetPassiveTransactionsQueryHandlerTests()
        {
            _mockRepository = new Mock<IPassiveTransactionRepository>();
            _mockLogger = new Mock<ILogger<GetPassiveTransactionsQueryHandler>>();
            _handler = new GetPassiveTransactionsQueryHandler(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_WithValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            var portfolioId = 123;
            var operationTypeId = 1;
            var query = new GetPassiveTransactionsQuery(portfolioId, operationTypeId);

            var expectedTransaction = new Domain.PassiveTransactions.PassiveTransaction();

            _mockRepository
                .Setup(repo => repo.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedTransaction);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(expectedTransaction.PassiveTransactionId, result.Value.PassiveTransactionId);
            Assert.Equal(expectedTransaction.DebitAccount, result.Value.DebitAccount);
            Assert.Equal(expectedTransaction.CreditAccount, result.Value.CreditAccount);
            Assert.Equal(expectedTransaction.ContraCreditAccount, result.Value.ContraCreditAccount);
            Assert.Equal(expectedTransaction.ContraDebitAccount, result.Value.ContraDebitAccount);
        }

        [Fact]
        public async Task Handle_WithNonExistentPortfolio_ReturnsEmptyResponse()
        {
            // Arrange
            var portfolioId = 999;
            var operationTypeId = 1;
            var query = new GetPassiveTransactionsQuery(portfolioId, operationTypeId);

            var emptyTransaction = new Domain.PassiveTransactions.PassiveTransaction();

            _mockRepository
                .Setup(repo => repo.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyTransaction);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Null(result.Value.DebitAccount);
            Assert.Null(result.Value.CreditAccount);
            Assert.Null(result.Value.ContraCreditAccount);
            Assert.Null(result.Value.ContraDebitAccount);
        }

        [Fact]
        public async Task Handle_WithRepositoryException_ThrowsException()
        {
            // Arrange
            var portfolioId = 123;
            var operationTypeId = 1;
            var query = new GetPassiveTransactionsQuery(portfolioId, operationTypeId);
            var expectedException = new Exception("Database connection failed");

            _mockRepository
                .Setup(repo => repo.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
            Assert.Equal(expectedException.Message, exception.Message);
        }

        [Fact]
        public async Task Handle_WithNullAccounts_HandlesNullValues()
        {
            // Arrange
            var portfolioId = 123;
            var operationTypeId = 1;
            var query = new GetPassiveTransactionsQuery(portfolioId, operationTypeId);

            var transactionWithNulls = new Domain.PassiveTransactions.PassiveTransaction();

            _mockRepository
                .Setup(repo => repo.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionWithNulls);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Null(result.Value.DebitAccount);
            Assert.Null(result.Value.CreditAccount);
            Assert.Null(result.Value.ContraCreditAccount);
            Assert.Null(result.Value.ContraDebitAccount);
        }

        [Fact]
        public void GetPassiveTransactionsResponse_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var passiveTransactionId = new long();
            var debitAccount = "DEBIT-123";
            var creditAccount = "CREDIT-456";
            var contraCreditAccount = "CONTRA-CREDIT-789";
            var contraDebitAccount = "CONTRA-DEBIT-012";

            // Act
            var response = new GetPassiveTransactionsResponse(
                passiveTransactionId,
                debitAccount,
                creditAccount,
                contraCreditAccount,
                contraDebitAccount);

            // Assert
            Assert.Equal(debitAccount, response.DebitAccount);
            Assert.Equal(creditAccount, response.CreditAccount);
            Assert.Equal(contraCreditAccount, response.ContraCreditAccount);
            Assert.Equal(contraDebitAccount, response.ContraDebitAccount);
        }

        [Fact]
        public void GetPassiveTransactionsResponse_WithNullValues_AllowsNull()
        {
            // Arrange & Act
            var response = new GetPassiveTransactionsResponse(
                new long(),
                null,
                null,
                null,
                null);

            // Assert
            Assert.Null(response.DebitAccount);
            Assert.Null(response.CreditAccount);
            Assert.Null(response.ContraCreditAccount);
            Assert.Null(response.ContraDebitAccount);
        }

        [Fact]
        public void GetPassiveTransactionsResponse_WithEmptyStrings_HandlesCorrectly()
        {
            // Arrange & Act
            var response = new GetPassiveTransactionsResponse(
                new long(),
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);

            // Assert
            Assert.Equal(string.Empty, response.DebitAccount);
            Assert.Equal(string.Empty, response.CreditAccount);
            Assert.Equal(string.Empty, response.ContraCreditAccount);
            Assert.Equal(string.Empty, response.ContraDebitAccount);
        }
    }
}