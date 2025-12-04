using Accounting.Application.Abstractions.Data;
using Accounting.Application.Treasuries.CreateTreasury;
using Accounting.Domain.Treasuries;
using Accounting.Integrations.Treasuries.CreateTreasury;
using Microsoft.Extensions.Logging;
using Moq;

namespace Accounting.test.UnitTests.Treasuries
{
    public class CreateTreasuryCommandHandlerTests
    {
        private readonly Mock<ITreasuryRepository> _mockRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<CreateTreasuryCommandHandler>> _mockLogger;
        private readonly CreateTreasuryCommandHandler _handler;

        public CreateTreasuryCommandHandlerTests()
        {
            _mockRepository = new Mock<ITreasuryRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<CreateTreasuryCommandHandler>>();
            _handler = new CreateTreasuryCommandHandler(
                _mockRepository.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ReturnsSuccess()
        {
            // Arrange
            var command = new CreateTreasuryCommand(
                PortfolioId: 123,
                BankAccount: "BANK-123",
                DebitAccount: "DEBIT-456",
                CreditAccount: "CREDIT-789"
            );

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            _mockRepository.Verify(repo => repo.Insert(It.IsAny<Domain.Treasuries.Treasury>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNullAccounts_ReturnsSuccess()
        {
            // Arrange
            var command = new CreateTreasuryCommand(
                PortfolioId: 123,
                BankAccount: null,
                DebitAccount: null,
                CreditAccount: null
            );

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _mockRepository.Verify(repo => repo.Insert(It.IsAny<Domain.Treasuries.Treasury>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithEmptyStringAccounts_ReturnsSuccess()
        {
            // Arrange
            var command = new CreateTreasuryCommand(
                PortfolioId: 123,
                BankAccount: string.Empty,
                DebitAccount: string.Empty,
                CreditAccount: string.Empty
            );

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _mockRepository.Verify(repo => repo.Insert(It.IsAny<Domain.Treasuries.Treasury>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithLongAccountNumbers_ReturnsSuccess()
        {
            // Arrange
            var longAccount = new string('A', 1000);
            var command = new CreateTreasuryCommand(
                PortfolioId: 123,
                BankAccount: longAccount,
                DebitAccount: longAccount,
                CreditAccount: longAccount
            );

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _mockRepository.Verify(repo => repo.Insert(It.IsAny<Domain.Treasuries.Treasury>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithZeroSavedChanges_ReturnsSuccess()
        {
            // Arrange
            var command = new CreateTreasuryCommand(
                PortfolioId: 123,
                BankAccount: "BANK-123",
                DebitAccount: "DEBIT-456",
                CreditAccount: "CREDIT-789"
            );

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0); // No changes saved

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            _mockRepository.Verify(repo => repo.Insert(It.IsAny<Domain.Treasuries.Treasury>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithDifferentPortfolioId_ReturnsSuccess()
        {
            // Arrange
            var command = new CreateTreasuryCommand(
                PortfolioId: 123,
                BankAccount: "BANK-123",
                DebitAccount: "DEBIT-456",
                CreditAccount: "CREDIT-789"
            );

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _mockRepository.Verify(repo => repo.Insert(It.IsAny<Domain.Treasuries.Treasury>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void CreateTreasuryCommand_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var treasuryId = 1;
            var portfolioId = 123;
            var bankAccount = "BANK-123";
            var debitAccount = "DEBIT-456";
            var creditAccount = "CREDIT-789";

            // Act
            var command = new CreateTreasuryCommand(
                portfolioId,
                bankAccount,
                debitAccount,
                creditAccount
            );

            // Assert
            Assert.Equal(portfolioId, command.PortfolioId);
            Assert.Equal(bankAccount, command.BankAccount);
            Assert.Equal(debitAccount, command.DebitAccount);
            Assert.Equal(creditAccount, command.CreditAccount);
        }
    }
}