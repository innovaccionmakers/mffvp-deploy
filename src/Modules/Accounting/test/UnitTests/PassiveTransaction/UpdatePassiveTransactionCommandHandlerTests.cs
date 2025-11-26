using Accounting.Application.Abstractions.Data;
using Accounting.Application.PassiveTransaction.UpdatePassiveTransaction;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.PassiveTransaction.UpdatePassiveTransaction;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Accounting.test.UnitTests.PassiveTransaction
{
    public class UpdatePassiveTransactionCommandHandlerTests
    {
        private readonly Mock<IPassiveTransactionRepository> _mockRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<UpdatePassiveTransactionCommandHandler>> _mockLogger;
        private readonly UpdatePassiveTransactionCommandHandler _handler;

        public UpdatePassiveTransactionCommandHandlerTests()
        {
            _mockRepository = new Mock<IPassiveTransactionRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<UpdatePassiveTransactionCommandHandler>>();
            _handler = new UpdatePassiveTransactionCommandHandler(
                _mockRepository.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_WithExistingTransaction_UpdatesSuccessfully()
        {
            // Arrange
            var portfolioId = 123;
            var operationTypeId = 1;
            var command = new UpdatePassiveTransactionCommand(
                PortfolioId: portfolioId,
                TypeOperationsId: operationTypeId,
                DebitAccount: "UPDATED-DEBIT",
                CreditAccount: "UPDATED-CREDIT",
                ContraCreditAccount: "UPDATED-CONTRA-CREDIT",
                ContraDebitAccount: "UPDATED-CONTRA-DEBIT"
            );

            var existingTransaction = Domain.PassiveTransactions.PassiveTransaction.Create(
                123,
                1,
                "OLD-DEBIT",
                "OLD-CREDIT",
                "OLD-CONTRA-CREDIT",
                "OLD-CONTRA-DEBIT"
            ).Value;

            _mockRepository
                .Setup(repo => repo.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTransaction);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Transacción pasiva actualizada correctamente.", result.Description);

            _mockRepository.Verify(repo => repo.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(repo => repo.Update(existingTransaction), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistingTransaction_ReturnsSuccessWithMessage()
        {
            // Arrange
            var portfolioId = 999;
            var operationTypeId = 1;
            var command = new UpdatePassiveTransactionCommand(
                PortfolioId: portfolioId,
                TypeOperationsId: operationTypeId,
                DebitAccount: "DEBIT-123",
                CreditAccount: "CREDIT-456",
                ContraCreditAccount: "CONTRA-CREDIT-789",
                ContraDebitAccount: "CONTRA-DEBIT-012"
            );

            _mockRepository
                .Setup(repo => repo.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.PassiveTransactions.PassiveTransaction)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("No hay transacción pasiva.", result.Description);

            _mockRepository.Verify(repo => repo.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(repo => repo.Update(It.IsAny<Domain.PassiveTransactions.PassiveTransaction>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithNullAccounts_UpdatesSuccessfully()
        {
            // Arrange
            var portfolioId = 123;
            var operationTypeId = 1;
            var command = new UpdatePassiveTransactionCommand(
                PortfolioId: portfolioId,
                TypeOperationsId: operationTypeId,
                DebitAccount: null,
                CreditAccount: null,
                ContraCreditAccount: null,
                ContraDebitAccount: null
            );

            var existingTransaction = Domain.PassiveTransactions.PassiveTransaction.Create(
                123,
                1,
                "OLD-DEBIT",
                "OLD-CREDIT",
                "OLD-CONTRA-CREDIT",
                "OLD-CONTRA-DEBIT"
            ).Value;

            _mockRepository
                .Setup(repo => repo.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTransaction);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _mockRepository.Verify(repo => repo.Update(existingTransaction), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithEmptyStringAccounts_UpdatesSuccessfully()
        {
            // Arrange
            var portfolioId = 123;
            var operationTypeId = 1;
            var command = new UpdatePassiveTransactionCommand(
                PortfolioId: portfolioId,
                TypeOperationsId: operationTypeId,
                DebitAccount: string.Empty,
                CreditAccount: string.Empty,
                ContraCreditAccount: string.Empty,
                ContraDebitAccount: string.Empty
            );

            var existingTransaction = Domain.PassiveTransactions.PassiveTransaction.Create(
                123,
                1,
                "OLD-DEBIT",
                "OLD-CREDIT",
                "OLD-CONTRA-CREDIT",
                "OLD-CONTRA-DEBIT"
            ).Value;

            _mockRepository
                .Setup(repo => repo.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTransaction);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _mockRepository.Verify(repo => repo.Update(existingTransaction), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithLongAccountNumbers_UpdatesSuccessfully()
        {
            // Arrange
            var portfolioId = 123;
            var operationTypeId = 1;
            var longAccountNumber = new string('A', 1000);
            var command = new UpdatePassiveTransactionCommand(
                PortfolioId: portfolioId,
                TypeOperationsId: operationTypeId,
                DebitAccount: longAccountNumber,
                CreditAccount: longAccountNumber,
                ContraCreditAccount: longAccountNumber,
                ContraDebitAccount: longAccountNumber
            );

            var existingTransaction = Domain.PassiveTransactions.PassiveTransaction.Create(
                123,
                1,
                "OLD-DEBIT",
                "OLD-CREDIT",
                "OLD-CONTRA-CREDIT",
                "OLD-CONTRA-DEBIT"
            ).Value;

            _mockRepository
                .Setup(repo => repo.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTransaction);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _mockRepository.Verify(repo => repo.Update(existingTransaction), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithDifferentOperationTypes_UpdatesSuccessfully()
        {
            // Arrange
            var portfolioId = 123;
            var operationTypeId = 2; // Different operation type
            var command = new UpdatePassiveTransactionCommand(
                PortfolioId: portfolioId,
                TypeOperationsId: operationTypeId,
                DebitAccount: "DEBIT-OP2",
                CreditAccount: "CREDIT-OP2",
                ContraCreditAccount: "CONTRA-CREDIT-OP2",
                ContraDebitAccount: "CONTRA-DEBIT-OP2"
            );

            var existingTransaction = Domain.PassiveTransactions.PassiveTransaction.Create(
                123,
                2,
                "OLD-DEBIT-OP2",
                "OLD-CREDIT-OP2",
                "OLD-CONTRA-CREDIT-OP2",
                "OLD-CONTRA-DEBIT-OP2"
            ).Value;

            _mockRepository
                .Setup(repo => repo.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTransaction);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _mockRepository.Verify(repo => repo.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(repo => repo.Update(existingTransaction), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithSaveChangesFailure_ThrowsException()
        {
            // Arrange
            var portfolioId = 123;
            var operationTypeId = 1;
            var command = new UpdatePassiveTransactionCommand(
                PortfolioId: portfolioId,
                TypeOperationsId: operationTypeId,
                DebitAccount: "UPDATED-DEBIT",
                CreditAccount: "UPDATED-CREDIT",
                ContraCreditAccount: "UPDATED-CONTRA-CREDIT",
                ContraDebitAccount: "UPDATED-CONTRA-DEBIT"
            );

            var existingTransaction = Domain.PassiveTransactions.PassiveTransaction.Create(
                123,
                1,
                "OLD-DEBIT",
                "OLD-CREDIT",
                "OLD-CONTRA-CREDIT",
                "OLD-CONTRA-DEBIT"
            ).Value;

            var expectedException = new Exception("Database save failed");

            _mockRepository
                .Setup(repo => repo.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTransaction);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal(expectedException.Message, exception.Message);

            _mockRepository.Verify(repo => repo.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(repo => repo.Update(existingTransaction), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithRepositoryException_LogsErrorAndThrows()
        {
            // Arrange
            var portfolioId = 123;
            var operationTypeId = 1;
            var command = new UpdatePassiveTransactionCommand(
                PortfolioId: portfolioId,
                TypeOperationsId: operationTypeId,
                DebitAccount: "UPDATED-DEBIT",
                CreditAccount: "UPDATED-CREDIT",
                ContraCreditAccount: "UPDATED-CONTRA-CREDIT",
                ContraDebitAccount: "UPDATED-CONTRA-DEBIT"
            );

            var expectedException = new Exception("Repository get failed");
            _mockRepository
                .Setup(repo => repo.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal(expectedException.Message, exception.Message);

            // Verify that the error was logged
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error al actualizar la transacción pasiva")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithZeroSavedChanges_ReturnsSuccess()
        {
            // Arrange
            var portfolioId = 123;
            var operationTypeId = 1;
            var command = new UpdatePassiveTransactionCommand(
                PortfolioId: portfolioId,
                TypeOperationsId: operationTypeId,
                DebitAccount: "UPDATED-DEBIT",
                CreditAccount: "UPDATED-CREDIT",
                ContraCreditAccount: "UPDATED-CONTRA-CREDIT",
                ContraDebitAccount: "UPDATED-CONTRA-DEBIT"
            );

            var existingTransaction = Domain.PassiveTransactions.PassiveTransaction.Create(
                123,
                1,
                "OLD-DEBIT",
                "OLD-CREDIT",
                "OLD-CONTRA-CREDIT",
                "OLD-CONTRA-DEBIT"
            ).Value;

            _mockRepository
                .Setup(repo => repo.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTransaction);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0); // No changes saved

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Transacción pasiva actualizada correctamente.", result.Description);

            _mockRepository.Verify(repo => repo.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(repo => repo.Update(existingTransaction), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void UpdatePassiveTransactionCommand_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var portfolioId = 123;
            var typeOperationsId = 1;
            var debitAccount = "DEBIT-123";
            var creditAccount = "CREDIT-456";
            var contraCreditAccount = "CONTRA-CREDIT-789";
            var contraDebitAccount = "CONTRA-DEBIT-012";

            // Act
            var command = new UpdatePassiveTransactionCommand(
                portfolioId,
                typeOperationsId,
                debitAccount,
                creditAccount,
                contraCreditAccount,
                contraDebitAccount
            );

            // Assert
            Assert.Equal(portfolioId, command.PortfolioId);
            Assert.Equal(typeOperationsId, command.TypeOperationsId);
            Assert.Equal(debitAccount, command.DebitAccount);
            Assert.Equal(creditAccount, command.CreditAccount);
            Assert.Equal(contraCreditAccount, command.ContraCreditAccount);
            Assert.Equal(contraDebitAccount, command.ContraDebitAccount);
        }

        [Fact]
        public void UpdatePassiveTransactionCommand_WithNullValues_AllowsNull()
        {
            // Arrange & Act
            var command = new UpdatePassiveTransactionCommand(
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
        public void UpdatePassiveTransactionCommand_WithEmptyStrings_HandlesCorrectly()
        {
            // Arrange & Act
            var command = new UpdatePassiveTransactionCommand(
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