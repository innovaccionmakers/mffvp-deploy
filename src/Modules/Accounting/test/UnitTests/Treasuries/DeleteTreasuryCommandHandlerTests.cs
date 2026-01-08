using Accounting.Application.Abstractions.Data;
using Accounting.Application.Abstractions.External;
using Accounting.Application.Treasuries.DeleteTreasury;
using Accounting.Domain.Treasuries;
using Accounting.Integrations.Treasuries.DeleteTreasury;
using Microsoft.Extensions.Logging;
using Moq;

namespace Accounting.test.UnitTests.Treasuries
{
    public class DeleteTreasuryCommandHandlerTests
    {
        private readonly Mock<ITreasuryRepository> _mockRepository;
        private readonly Mock<IPortfolioLocator> _mockPortfolioLocator;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<DeleteTreasuryCommandHandler>> _mockLogger;
        private readonly DeleteTreasuryCommandHandler _handler;

        public DeleteTreasuryCommandHandlerTests()
        {
            _mockRepository = new Mock<ITreasuryRepository>();
            _mockPortfolioLocator = new Mock<IPortfolioLocator>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<DeleteTreasuryCommandHandler>>();

            _handler = new DeleteTreasuryCommandHandler(
                _mockRepository.Object,
                _mockPortfolioLocator.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object);
        }

        [Fact]
        public void DeleteTreasuryCommand_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var portfolioId = 123;
            var bankAccount = "ACC001";

            // Act
            var command = new DeleteTreasuryCommand(portfolioId, bankAccount);

            // Assert
            Assert.Equal(bankAccount, command.BankAccount);
            Assert.Equal(portfolioId, command.PortfolioId);
        }

        [Fact]
        public void DeleteTreasuryCommand_WithMinimumValues_HandlesCorrectly()
        {
            // Arrange & Act
            var command = new DeleteTreasuryCommand(0, string.Empty);

            // Assert
            Assert.Equal(string.Empty, command.BankAccount);
            Assert.Equal(0, command.PortfolioId);
        }

        [Fact]
        public void DeleteTreasuryCommand_WithMaximumValues_HandlesCorrectly()
        {
            // Arrange & Act
            var command = new DeleteTreasuryCommand(int.MaxValue, "ACC001");

            // Assert
            Assert.Equal("ACC001", command.BankAccount);
            Assert.Equal(int.MaxValue, command.PortfolioId);
        }
    }
}