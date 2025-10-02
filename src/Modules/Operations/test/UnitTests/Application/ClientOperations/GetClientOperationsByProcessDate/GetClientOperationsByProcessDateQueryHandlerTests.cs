using Common.SharedKernel.Domain.OperationTypes;
using Operations.Integrations.ClientOperations.GetAccountingOperations;

namespace Operations.test.UnitTests.Application.ClientOperations.GetClientOperationsByProcessDate
{
    public class GetClientOperationsByProcessDateQueryHandlerTests
    {

        [Fact]
        public void GetAccountingOperationsResponse_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var portfolioId = new int();
            var affiliateId = new int();
            var amount = 1500.75m;
            var operationTypeName = "Transfer";
            var operationTypeId = new int();
            var collectionAccount = "ACC123456";
            IncomeEgressNature nature = new IncomeEgressNature();

            // Act
            var response = new GetAccountingOperationsResponse(
                portfolioId,
                affiliateId,
                amount,
                operationTypeName,
                nature,
                operationTypeId,
                collectionAccount);

            // Assert
            Assert.Equal(portfolioId, response.PortfolioId);
            Assert.Equal(affiliateId, response.AffiliateId);
            Assert.Equal(amount, response.Amount);
            Assert.Equal(operationTypeId, response.OperationTypeId);
            Assert.Equal(collectionAccount, response.CollectionAccount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100.50)]
        [InlineData(-50.25)]
        [InlineData(999999.99)]
        public void GetAccountingOperationsResponse_WithDifferentAmounts_SetsAmountCorrectly(decimal amount)
        {
            // Arrange & Act
            var response = new GetAccountingOperationsResponse(
                new int(),
                new int(),
                amount,
                "TestOperation",
                new IncomeEgressNature(),
                new int(),
                "TestAccount");

            // Assert
            Assert.Equal(amount, response.Amount);
        }

        [Fact]
        public void GetAccountingOperationsResponse_WithNullCollectionAccount_AllowsNull()
        {
            // Arrange & Act
            var response = new GetAccountingOperationsResponse(
                new int(),
                new int(),
                100m,
                "TestOperation",
                new IncomeEgressNature(),
                new int(),
                null);

            // Assert
            Assert.Null(response.CollectionAccount);
        }
    }
}

