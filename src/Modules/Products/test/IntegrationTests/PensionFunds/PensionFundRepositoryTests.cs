using Moq;
using Products.Domain.PensionFunds;

namespace Products.test.IntegrationTests.PensionFunds
{
    public class PensionFundRepositoryTests
    {
        private readonly Mock<IPensionFundRepository> _pensionFundRepository;

        public PensionFundRepositoryTests()
        {
            _pensionFundRepository = new Mock<IPensionFundRepository>();
        }

        [Fact]
        public async Task GetAllAsync_WhenNoPensionFunds_ShouldReturnEmptyCollection()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _pensionFundRepository.Object.GetAllPensionFundsAsync(cancellationToken);

            // Assert
            Assert.Null(result);
        }
    }
}
