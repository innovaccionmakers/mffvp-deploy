using Accounting.Application.Treasury.GetTreasuries;
using Accounting.Domain.Treasuries;
using Accounting.Integrations.Treasury.GetTreasuries;
using Microsoft.Extensions.Logging;
using Moq;

namespace Accounting.test.UnitTests.Treasuries
{
    public class GetTreasuryQueryHandlerTests
    {
        private readonly Mock<ITreasuryRepository> _mockRepository;
        private readonly Mock<ILogger<GetTreasuryQueryHandler>> _mockLogger;
        private readonly GetTreasuryQueryHandler _handler;

        public GetTreasuryQueryHandlerTests()
        {
            _mockRepository = new Mock<ITreasuryRepository>();
            _mockLogger = new Mock<ILogger<GetTreasuryQueryHandler>>();
            _handler = new GetTreasuryQueryHandler(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetTreasuryResponse_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var treasuryId = 1;
            var bankAccount = "BANK-123";
            var debitAccount = "DEBIT-456";
            var creditAccount = "CREDIT-789";

            // Act
            var response = new GetTreasuryResponse(treasuryId, bankAccount, debitAccount, creditAccount);

            // Assert
            Assert.Equal(treasuryId, response.TreasuryId);
            Assert.Equal(bankAccount, response.BankAccount);
            Assert.Equal(debitAccount, response.DebitAccount);
            Assert.Equal(creditAccount, response.CreditAccount);
        }

        [Fact]
        public void GetTreasuryResponse_WithNullAccounts_AllowsNull()
        {
            // Arrange & Act
            var response = new GetTreasuryResponse(1, null, null, null);

            // Assert
            Assert.Null(response.BankAccount);
            Assert.Null(response.DebitAccount);
            Assert.Null(response.CreditAccount);
        }

        [Fact]
        public void GetTreasuryResponse_WithEmptyAccounts_AllowsEmptyStrings()
        {
            // Arrange & Act
            var response = new GetTreasuryResponse(1, string.Empty, string.Empty, string.Empty);

            // Assert
            Assert.Equal(string.Empty, response.BankAccount);
            Assert.Equal(string.Empty, response.DebitAccount);
            Assert.Equal(string.Empty, response.CreditAccount);
        }

        [Fact]
        public void GetTreasuryResponse_WithLongAccountNumbers_HandlesCorrectly()
        {
            // Arrange
            var longAccount = new string('A', 1000);

            // Act
            var response = new GetTreasuryResponse(1, longAccount, longAccount, longAccount);

            // Assert
            Assert.Equal(1000, response.BankAccount.Length);
            Assert.Equal(1000, response.DebitAccount.Length);
            Assert.Equal(1000, response.CreditAccount.Length);
        }

        [Fact]
        public void GetTreasuryResponse_WithMinTreasuryId_HandlesCorrectly()
        {
            // Arrange & Act
            var response = new GetTreasuryResponse(0, "BANK-123", "DEBIT-456", "CREDIT-789");

            // Assert
            Assert.Equal(0, response.TreasuryId);
        }

        [Fact]
        public void GetTreasuryResponse_WithMaxTreasuryId_HandlesCorrectly()
        {
            // Arrange & Act
            var response = new GetTreasuryResponse(int.MaxValue, "BANK-123", "DEBIT-456", "CREDIT-789");

            // Assert
            Assert.Equal(int.MaxValue, response.TreasuryId);
        }
    }
}