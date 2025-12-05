using Accounting.Integrations.Treasuries.GetAccountingOperationsTreasuries;

namespace Accounting.test.UnitTests.Treasuries
{
    public class GetAccountingOperationsHandlerTests
    {
        [Fact]
        public void GetAccountingOperationsTreasuriesResponse_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var portfolioId = new int();
            var debitAccount = "DEBIT-12345";

            // Act
            var response = new GetAccountingOperationsTreasuriesResponse(portfolioId, debitAccount, null);

            // Assert
            Assert.Equal(portfolioId, response.PortfolioId);
            Assert.Equal(debitAccount, response.DebitAccount);
        }

        [Fact]
        public void GetAccountingOperationsTreasuriesResponse_WithNullDebitAccount_AllowsNull()
        {
            // Arrange & Act
            var response = new GetAccountingOperationsTreasuriesResponse(new int(), null, null);

            // Assert
            Assert.Null(response.DebitAccount);
        }

        [Fact]
        public void GetAccountingOperationsTreasuriesResponse_WithEmptyDebitAccount_AllowsEmptyString()
        {
            // Arrange & Act
            var response = new GetAccountingOperationsTreasuriesResponse(new int(), string.Empty, null);

            // Assert
            Assert.Equal(string.Empty, response.DebitAccount);
        }

        [Fact]
        public void GetAccountingOperationsTreasuriesResponse_WithMinGuid_HandlesCorrectly()
        {
            // Arrange & Act
            var response = new GetAccountingOperationsTreasuriesResponse(new int(), "DEBIT-001", null);

            // Assert
            Assert.Equal(new int(), response.PortfolioId);
        }

        [Theory]
        [InlineData("DEBIT-001")]
        [InlineData("1234567890")]
        [InlineData("ACC-DEBIT-GHI")]
        [InlineData("debit_with_underscore")]
        [InlineData("debit-with-dash")]
        [InlineData("DEBIT.123.456")]
        [InlineData("DBT-12345")]
        public void GetAccountingOperationsTreasuriesResponse_WithVariousDebitAccountFormats_HandlesCorrectly(string debitAccount)
        {
            // Arrange & Act
            var response = new GetAccountingOperationsTreasuriesResponse(new int(), debitAccount, null);

            // Assert
            Assert.Equal(debitAccount, response.DebitAccount);
        }

        [Fact]
        public void GetAccountingOperationsTreasuriesResponse_WithLongDebitAccount_HandlesCorrectly()
        {
            // Arrange
            var longDebitAccount = new string('A', 1000);

            // Act
            var response = new GetAccountingOperationsTreasuriesResponse(new int(), longDebitAccount, null);

            // Assert
            Assert.Equal(1000, response.DebitAccount.Length);
        }

        [Fact]
        public void GetAccountingOperationsTreasuriesResponse_WithSpecialCharactersInDebitAccount_HandlesCorrectly()
        {
            // Arrange
            var debitAccount = "DEBIT-123";

            // Act
            var response = new GetAccountingOperationsTreasuriesResponse(new int(), debitAccount, null);

            // Assert
            Assert.Equal(debitAccount, response.DebitAccount);
        }

        [Fact]
        public void GetAccountingOperationsTreasuriesResponse_WithUnicodeCharactersInDebitAccount_HandlesCorrectly()
        {
            // Arrange
            var debitAccount = "321654987";

            // Act
            var response = new GetAccountingOperationsTreasuriesResponse(new int(), debitAccount, null);

            // Assert
            Assert.Equal(debitAccount, response.DebitAccount);
        }
    }
}
