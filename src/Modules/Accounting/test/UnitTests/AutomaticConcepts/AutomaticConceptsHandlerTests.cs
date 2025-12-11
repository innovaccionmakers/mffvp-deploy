using Accounting.Application.Abstractions.External;
using Accounting.Application.AutomaticConcepts;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.AutomaticConcepts;
using Closing.IntegrationEvents.Yields;
using Closing.Integrations.YieldDetails;
using Closing.Integrations.Yields.Queries;
using Microsoft.Extensions.Logging;
using Moq;
using Operations.IntegrationEvents.OperationTypes;
using Operations.Integrations.OperationTypes;
using Products.Integrations.Portfolios;

namespace Accounting.test.UnitTests.AutomaticConcepts
{
    public class AutomaticConceptsHandlerValidatorTests
    {
        private readonly Mock<IPortfolioLocator> _portfolioLocatorMock;
        private readonly Mock<IOperationLocator> _operationLocatorMock;
        private readonly Mock<IPassiveTransactionRepository> _passiveTransactionRepositoryMock;
        private readonly Mock<ILogger<AutomaticConceptsHandlerValidator>> _loggerMock;
        private readonly AutomaticConceptsHandlerValidator _validator;

        public AutomaticConceptsHandlerValidatorTests()
        {
            _portfolioLocatorMock = new Mock<IPortfolioLocator>();
            _operationLocatorMock = new Mock<IOperationLocator>();
            _passiveTransactionRepositoryMock = new Mock<IPassiveTransactionRepository>();
            _loggerMock = new Mock<ILogger<AutomaticConceptsHandlerValidator>>();

            _validator = new AutomaticConceptsHandlerValidator(
                _portfolioLocatorMock.Object,
                _operationLocatorMock.Object,
                _passiveTransactionRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task AutomaticConceptsValidator_WhenYieldsHaveNoCreditDifference_ReturnsEmptyResults()
        {
            // Arrange            
            var yieldResult = new GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerResponse(
                true, // IsValid
                "200", // Code
                "Success", // Message
                new YieldAutConceptsCompleteResponse(
                    new List<YieldAutConceptsResponse>
                    {
                        new YieldAutConceptsResponse(1, 1, 100, 100, 100) // YieldId, PortfolioId, YieldToCredit, CreditedYields, YieldToDistributedValue
                    },
                    new List<YieldDetailsAutConceptsResponse>()
                )
            );


            var automaticConcept = "TestConcept";
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _validator.AutomaticConceptsValidator(
                DateTime.UtcNow, yieldResult, new List<OperationTypeResponse>(), automaticConcept, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Errors);
        }
    }
}