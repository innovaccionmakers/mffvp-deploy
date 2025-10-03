using Accounting.Integrations.Treasuries.GetAccountingConceptsTreasuries;

namespace Accounting.test.UnitTests.Treasuries
{
    public class GetAccountingConceptsHandlerTests
    {
        [Fact]
        public void GetAccountingConceptsTreasuriesResponse_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var portfolioId = new int();
            var debitAccount = "DEBIT-12345";
            var creditAccount = "CREDIT-12345";

            // Act
            var response = new GetAccountingConceptsTreasuriesResponse(portfolioId, debitAccount, creditAccount);

            // Assert
            Assert.Equal(portfolioId, response.PortfolioId);
            Assert.Equal(debitAccount, response.DebitAccount);
        }

        [Fact]
        public void GetAccountingConceptsTreasuriesResponse_WithNullDebitAccount_AllowsNull()
        {
            // Arrange & Act
            var response = new GetAccountingConceptsTreasuriesResponse(new int(), null, null);

            // Assert
            Assert.Null(response.DebitAccount);
        }

        [Fact]
        public void GetAccountingConceptsTreasuriesResponse_WithEmptyDebitAccount_AllowsEmptyString()
        {
            // Arrange & Act
            var response = new GetAccountingConceptsTreasuriesResponse(new int(), string.Empty, string.Empty);

            // Assert
            Assert.Equal(string.Empty, response.DebitAccount);
        }

        [Fact]
        public void GetAccountingConceptsTreasuriesResponse_WithMinGuid_HandlesCorrectly()
        {
            // Arrange & Act
            var response = new GetAccountingConceptsTreasuriesResponse(new int(), "DEBIT-001", "CREDIT-001");

            // Assert
            Assert.Equal(new int(), response.PortfolioId);
        }

        [Fact]
        public void GetAccountingConceptsTreasuriesResponse_WithLongConceptAccount_HandlesCorrectly()
        {
            // Arrange
            var longDebitAccount = new string('D', 500); 
            var longCreditAccount = new string('C', 500);            

            // Act
            var response = new GetAccountingConceptsTreasuriesResponse(new int(), longDebitAccount, longDebitAccount);

            // Assert
            Assert.Equal(500, response.DebitAccount.Length);
        }

        [Fact]
        public void GetAccountingConceptsTreasuriesResponse_WithFinancialConceptFormat_HandlesCorrectly()
        {
            // Arrange
            var debitAccount = "4.1.1.01.001";
            var creditAccount = "4.1.1.01.001";

            // Act
            var response = new GetAccountingConceptsTreasuriesResponse(new int(), debitAccount, creditAccount);

            // Assert
            Assert.Equal(debitAccount, response.DebitAccount);
        }
    }
}
