using Operations.Integrations.ClientOperations;
using System.Text.Json;

namespace Operations.test.UnitTests.Application.ClientOperations.GetAccountingOperations
{
    public class GetAccountingOperationsQueryHandlerTests
    {
        [Fact]
        public void ClientOperationsByProcessDateResponse_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange        
            var amount = 1500.75m;
            var collectionAccount = "ACC123456";
            string jsonString = "{\"name\":\"John\", \"age\":30, \"city\":\"New York\"}";    
            var paymentMethodDetail = JsonDocument.Parse(jsonString);
            var operationTypeName = "Deposit";

            // Act
            var response = new ClientOperationsByProcessDateResponse(
                amount,
                collectionAccount,
                paymentMethodDetail,
                operationTypeName);

            // Assert
            Assert.Equal(amount, response.Amount);
            Assert.Equal(collectionAccount, response.CollectionAccount);
            Assert.Equal(paymentMethodDetail, response.PaymentMethodDetail);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100.50)]
        [InlineData(-50.25)]
        [InlineData(999999.99)]
        public void ClientOperationsByProcessDateResponse_WithDifferentAmounts_SetsAmountCorrectly(decimal amount)
        {
            // Arrange        
            var collectionAccount = "ACC123456";
            string jsonString = "{\"name\":\"John\", \"age\":30, \"city\":\"New York\"}";    
            var paymentMethodDetail = JsonDocument.Parse(jsonString);
            var operationTypeName = "Deposit";

            // Act
            var response = new ClientOperationsByProcessDateResponse(
                amount,
                collectionAccount,
                paymentMethodDetail,
                operationTypeName);

            // Assert
            Assert.Equal(amount, response.Amount);
        }

        [Fact]
        public void ClientOperationsByProcessDateResponse_WithNullPaymentMethodDetail_AllowsNull()
        {
            // Arrange & Act
            var response = new ClientOperationsByProcessDateResponse(
                100m,
                "ACC001",
                null,
                "Deposit");

            // Assert
            Assert.Null(response.PaymentMethodDetail);
        }


        [Fact]
        public void ClientOperationsByProcessDateResponse_WithAllNullOptionalFields_HandlesCorrectly()
        {
            // Arrange & Act
            var response = new ClientOperationsByProcessDateResponse(
                0m,
                null,
                null,
                string.Empty);

            // Assert
            Assert.Equal(0m, response.Amount);
            Assert.Null(response.CollectionAccount);
            Assert.Null(response.PaymentMethodDetail);
        }
    }
}
