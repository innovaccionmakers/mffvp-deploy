using Accounting.Application.Abstractions.Data;
using Accounting.Application.Treasuries.UpdateTreasury;
using Accounting.Domain.Treasuries;
using Accounting.Integrations.Treasuries.UpdateTreasury;
using Microsoft.Extensions.Logging;
using Moq;

namespace Accounting.test.UnitTests.Treasuries
{
    public class UpdateTreasuryCommandHandlerTests
    {
        private readonly Mock<ITreasuryRepository> _mockRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<UpdateTreasuryCommandHandler>> _mockLogger;
        private readonly UpdateTreasuryCommandHandler _handler;

        public UpdateTreasuryCommandHandlerTests()
        {
            _mockRepository = new Mock<ITreasuryRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<UpdateTreasuryCommandHandler>>();
            _handler = new UpdateTreasuryCommandHandler(
                _mockRepository.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object);
        }

        [Fact]
        public void UpdateTreasuryCommand_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var treasuryId = 1;
            var portfolioId = 123;
            var bankAccount = "BANK-123";
            var debitAccount = "DEBIT-456";
            var creditAccount = "CREDIT-789";

            // Act
            var command = new UpdateTreasuryCommand(
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

        [Fact]
        public void UpdateTreasuryCommand_WithNullValues_AllowsNull()
        {
            // Arrange & Act
            var command = new UpdateTreasuryCommand(
                123,
                null,
                null,
                null
            );

            // Assert
            Assert.Null(command.BankAccount);
            Assert.Null(command.DebitAccount);
            Assert.Null(command.CreditAccount);
        }

        [Fact]
        public void UpdateTreasuryCommand_WithEmptyStrings_HandlesCorrectly()
        {
            // Arrange & Act
            var command = new UpdateTreasuryCommand(
                123,
                string.Empty,
                string.Empty,
                string.Empty
            );

            // Assert
            Assert.Equal(string.Empty, command.BankAccount);
            Assert.Equal(string.Empty, command.DebitAccount);
            Assert.Equal(string.Empty, command.CreditAccount);
        }

        [Fact]
        public void UpdateTreasuryCommand_WithSpecialAccountFormats_HandlesCorrectly()
        {
            // Arrange & Act
            var command = new UpdateTreasuryCommand(
                123,
                "BANK.123.456",
                "DEBIT-789-ABC",
                "CREDIT_XYZ_123"
            );

            // Assert
            Assert.Equal("BANK.123.456", command.BankAccount);
            Assert.Equal("DEBIT-789-ABC", command.DebitAccount);
            Assert.Equal("CREDIT_XYZ_123", command.CreditAccount);
        }

        [Fact]
        public void UpdateTreasuryCommand_WithMinimumIds_HandlesCorrectly()
        {
            // Arrange & Act
            var command = new UpdateTreasuryCommand(
                0,
                "BANK-123",
                "DEBIT-456",
                "CREDIT-789"
            );

            // Assert
            Assert.Equal(0, command.PortfolioId);
        }

        [Fact]
        public void UpdateTreasuryCommand_WithMaximumIds_HandlesCorrectly()
        {
            // Arrange & Act
            var command = new UpdateTreasuryCommand(
                int.MaxValue,
                "BANK-123",
                "DEBIT-456",
                "CREDIT-789"
            );

            // Assert
            Assert.Equal(int.MaxValue, command.PortfolioId);
        }
    }
}