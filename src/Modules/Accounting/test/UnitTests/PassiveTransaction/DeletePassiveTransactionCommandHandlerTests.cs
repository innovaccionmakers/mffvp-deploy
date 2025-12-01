using Accounting.Application.Abstractions.Data;
using Accounting.Application.PassiveTransaction.DeletePassiveTransaction;
using Accounting.Application.PassiveTransaction.UpdatePassiveTransaction;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.PassiveTransaction.DeletePassiveTransaction;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Accounting.test.UnitTests.PassiveTransaction
{
    public class DeletePassiveTransactionCommandHandlerTests
    {
        private readonly Mock<IPassiveTransactionRepository> _mockRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<UpdatePassiveTransactionCommandHandler>> _mockLogger;
        private readonly DeletePassiveTransactionCommandHandler _handler;

        public DeletePassiveTransactionCommandHandlerTests()
        {
            _mockRepository = new Mock<IPassiveTransactionRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<UpdatePassiveTransactionCommandHandler>>();

            _handler = new DeletePassiveTransactionCommandHandler(
                _mockRepository.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_WithNonExistingTransaction_ReturnsFailure()
        {
            // Arrange
            var portfolioId = 999;
            var operationTypeId = 1;
            var command = new DeletePassiveTransactionCommand(portfolioId, operationTypeId);

            _mockRepository
                .Setup(repo => repo.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.PassiveTransactions.PassiveTransaction)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(Error.NullValue, result.Error);

            _mockRepository.Verify(repo => repo.GetByPortfolioIdAndOperationTypeAsync(portfolioId, operationTypeId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(repo => repo.Delete(It.IsAny<Domain.PassiveTransactions.PassiveTransaction>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void DeletePassiveTransactionCommand_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var portfolioId = 123;
            var typeOperationsId = 1;

            // Act
            var command = new DeletePassiveTransactionCommand(portfolioId, typeOperationsId);

            // Assert
            Assert.Equal(portfolioId, command.PortfolioId);
            Assert.Equal(typeOperationsId, command.TypeOperationsId);
        }

        [Fact]
        public void DeletePassiveTransactionCommand_WithMinimumValues_HandlesCorrectly()
        {
            // Arrange & Act
            var command = new DeletePassiveTransactionCommand(0, 0);

            // Assert
            Assert.Equal(0, command.PortfolioId);
            Assert.Equal(0, command.TypeOperationsId);
        }

        [Fact]
        public void DeletePassiveTransactionCommand_WithMaximumValues_HandlesCorrectly()
        {
            // Arrange & Act
            var command = new DeletePassiveTransactionCommand(int.MaxValue, int.MaxValue);

            // Assert
            Assert.Equal(int.MaxValue, command.PortfolioId);
            Assert.Equal(int.MaxValue, command.TypeOperationsId);
        }
    }
}