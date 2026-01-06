using Accounting.Application.Abstractions.Data;
using Accounting.Application.PassiveTransaction.CreatePassiveTransaction;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.PassiveTransaction.CreatePassiveTransaction;
using Microsoft.Extensions.Logging;
using Moq;

namespace Accounting.test.UnitTests.PassiveTransaction
{
    public class CreatePassiveTransactionCommandHandlerTests
    {
        private readonly Mock<IPassiveTransactionRepository> _mockRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<CreatePassiveTransactionCommandHandler>> _mockLogger;
        private readonly CreatePassiveTransactionCommandHandler _handler;

        public CreatePassiveTransactionCommandHandlerTests()
        {
            _mockRepository = new Mock<IPassiveTransactionRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<CreatePassiveTransactionCommandHandler>>();
            _handler = new CreatePassiveTransactionCommandHandler(
                _mockRepository.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ReturnsSuccessResult()
        {
            // Arrange
            var command = new CreatePassiveTransactionCommand(
                PortfolioId: 123,
                TypeOperationId: 1,
                DebitAccount: "DEBIT-123",
                CreditAccount: "CREDIT-456",
                ContraCreditAccount: "CONTRA-CREDIT-789",
                ContraDebitAccount: "CONTRA-DEBIT-012"
            );

            var passiveTransaction = Domain.PassiveTransactions.PassiveTransaction.Create(
                command.PortfolioId,
                command.TypeOperationId,
                command.DebitAccount,
                command.CreditAccount,
                command.ContraCreditAccount,
                command.ContraDebitAccount
            ).Value;

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            _mockRepository.Verify(repo => repo.Insert(It.IsAny<Domain.PassiveTransactions.PassiveTransaction>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNullAccounts_CreatesTransactionSuccessfully()
        {
            // Arrange
            var command = new CreatePassiveTransactionCommand(
                PortfolioId: 123,
                TypeOperationId: 1,
                DebitAccount: null,
                CreditAccount: null,
                ContraCreditAccount: null,
                ContraDebitAccount: null
            );

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _mockRepository.Verify(repo => repo.Insert(It.IsAny<Domain.PassiveTransactions.PassiveTransaction>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithEmptyStringAccounts_CreatesTransactionSuccessfully()
        {
            // Arrange
            var command = new CreatePassiveTransactionCommand(
                PortfolioId: 123,
                TypeOperationId: 1,
                DebitAccount: string.Empty,
                CreditAccount: string.Empty,
                ContraCreditAccount: string.Empty,
                ContraDebitAccount: string.Empty
            );

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _mockRepository.Verify(repo => repo.Insert(It.IsAny<Domain.PassiveTransactions.PassiveTransaction>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithLongAccountNumbers_CreatesTransactionSuccessfully()
        {
            // Arrange
            var longAccountNumber = new string('A', 1000);
            var command = new CreatePassiveTransactionCommand(
                PortfolioId: 123,
                TypeOperationId: 1,
                DebitAccount: longAccountNumber,
                CreditAccount: longAccountNumber,
                ContraCreditAccount: longAccountNumber,
                ContraDebitAccount: longAccountNumber
            );

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _mockRepository.Verify(repo => repo.Insert(It.IsAny<Domain.PassiveTransactions.PassiveTransaction>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithDifferentOperationTypes_CreatesTransactionSuccessfully()
        {
            // Arrange
            var command = new CreatePassiveTransactionCommand(
                PortfolioId: 123,
                TypeOperationId: 2, // Different operation type
                DebitAccount: "DEBIT-OP2",
                CreditAccount: "CREDIT-OP2",
                ContraCreditAccount: "CONTRA-CREDIT-OP2",
                ContraDebitAccount: "CONTRA-DEBIT-OP2"
            );

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _mockRepository.Verify(repo => repo.Insert(It.IsAny<Domain.PassiveTransactions.PassiveTransaction>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithZeroSavedChanges_ReturnsSuccess()
        {
            // Arrange
            var command = new CreatePassiveTransactionCommand(
                PortfolioId: 123,
                TypeOperationId: 1,
                DebitAccount: "DEBIT-123",
                CreditAccount: "CREDIT-456",
                ContraCreditAccount: "CONTRA-CREDIT-789",
                ContraDebitAccount: "CONTRA-DEBIT-012"
            );

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0); // No changes saved

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            _mockRepository.Verify(repo => repo.Insert(It.IsAny<Domain.PassiveTransactions.PassiveTransaction>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void CreatePassiveTransactionCommand_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var portfolioId = 123;
            var typeOperationsId = 1;
            var debitAccount = "DEBIT-123";
            var creditAccount = "CREDIT-456";
            var contraCreditAccount = "CONTRA-CREDIT-789";
            var contraDebitAccount = "CONTRA-DEBIT-012";

            // Act
            var command = new CreatePassiveTransactionCommand(
                portfolioId,
                typeOperationsId,
                debitAccount,
                creditAccount,
                contraCreditAccount,
                contraDebitAccount
            );

            // Assert
            Assert.Equal(portfolioId, command.PortfolioId);
            Assert.Equal(typeOperationsId, command.TypeOperationId);
            Assert.Equal(debitAccount, command.DebitAccount);
            Assert.Equal(creditAccount, command.CreditAccount);
            Assert.Equal(contraCreditAccount, command.ContraCreditAccount);
            Assert.Equal(contraDebitAccount, command.ContraDebitAccount);
        }

        [Fact]
        public void CreatePassiveTransactionCommand_WithNullValues_AllowsNull()
        {
            // Arrange & Act
            var command = new CreatePassiveTransactionCommand(
                123,
                1,
                null,
                null,
                null,
                null
            );

            // Assert
            Assert.Null(command.DebitAccount);
            Assert.Null(command.CreditAccount);
            Assert.Null(command.ContraCreditAccount);
            Assert.Null(command.ContraDebitAccount);
        }

        [Fact]
        public void CreatePassiveTransactionCommand_WithEmptyStrings_HandlesCorrectly()
        {
            // Arrange & Act
            var command = new CreatePassiveTransactionCommand(
                123,
                1,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty
            );

            // Assert
            Assert.Equal(string.Empty, command.DebitAccount);
            Assert.Equal(string.Empty, command.CreditAccount);
            Assert.Equal(string.Empty, command.ContraCreditAccount);
            Assert.Equal(string.Empty, command.ContraDebitAccount);
        }
    }
}