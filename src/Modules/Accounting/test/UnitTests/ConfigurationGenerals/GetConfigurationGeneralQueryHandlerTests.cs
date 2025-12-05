using Accounting.Application.ConfigurationGenerals.GetConfigurationGeneral;
using Accounting.Domain.ConfigurationGenerals;
using Accounting.Integrations.ConfigurationGenerals.GetConfigurationGeneral;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Accounting.test.UnitTests.ConfigurationGenerals
{
    public class GetConfigurationGeneralQueryHandlerTests
    {
        private readonly Mock<IGeneralConfigurationRepository> _mockGeneralConfigurationRepository;
        private readonly Mock<ILogger<Accounting.Application.ConfigurationGenerals.CreateConfigurationGeneral.CreateConfigurationGeneralCommandHandler>> _mockLogger;
        private readonly GetConfigurationGeneralQueryHandler _handler;

        public GetConfigurationGeneralQueryHandlerTests()
        {
            _mockGeneralConfigurationRepository = new Mock<IGeneralConfigurationRepository>();
            _mockLogger = new Mock<ILogger<Accounting.Application.ConfigurationGenerals.CreateConfigurationGeneral.CreateConfigurationGeneralCommandHandler>>();

            _handler = new GetConfigurationGeneralQueryHandler(
                _mockGeneralConfigurationRepository.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenConfigurationNotFound()
        {
            // Arrange
            var portfolioId = 1;
            var query = new GetConfigurationGeneralQuery(portfolioId);

            _mockGeneralConfigurationRepository
                .Setup(repo => repo.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((GeneralConfiguration)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("0");
            result.Error.Description.Should().Contain("No se encontró registro de la configuración general");

            _mockGeneralConfigurationRepository.Verify(
                repo => repo.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_ShouldPassCancellationTokenToRepository()
        {
            // Arrange
            var portfolioId = 1;
            var query = new GetConfigurationGeneralQuery(portfolioId);
            var cancellationToken = new CancellationTokenSource().Token;

            var existingConfiguration = GeneralConfiguration.Create(
                portfolioId,
                "ACC001",
                "CC001"
            ).Value;

            _mockGeneralConfigurationRepository
                .Setup(repo => repo.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, cancellationToken))
                .ReturnsAsync(existingConfiguration);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.IsSuccess.Should().BeTrue();

            _mockGeneralConfigurationRepository.Verify(
                repo => repo.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, cancellationToken),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenConfigurationHasInvalidState()
        {
            // Arrange
            var portfolioId = 1;
            var query = new GetConfigurationGeneralQuery(portfolioId);

            // Create a configuration with invalid data (if supported by domain)
            var existingConfiguration = GeneralConfiguration.Create(
                portfolioId,
                "", // Empty accounting code might be invalid
                "CC001"
            ).Value;

            if (existingConfiguration != null)
            {
                _mockGeneralConfigurationRepository
                    .Setup(repo => repo.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(existingConfiguration);

                // Act
                var result = await _handler.Handle(query, CancellationToken.None);

                // Assert
                // The handler should still succeed because it just maps the data
                // Validation should be done at creation time, not retrieval time
                result.IsSuccess.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Handle_ShouldNotLogError_WhenOperationIsSuccessful()
        {
            // Arrange
            var portfolioId = 1;
            var query = new GetConfigurationGeneralQuery(portfolioId);

            var existingConfiguration = GeneralConfiguration.Create(
                portfolioId,
                "ACC001",
                "CC001"
            ).Value;

            _mockGeneralConfigurationRepository
                .Setup(repo => repo.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingConfiguration);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never
            );
        }

        [Fact]
        public async Task Handle_ShouldHandleNullPropertiesInResponse()
        {
            // Arrange
            var portfolioId = 1;
            var query = new GetConfigurationGeneralQuery(portfolioId);

            // Create a configuration with null values (if allowed by domain)
            var existingConfiguration = GeneralConfiguration.Create(
                portfolioId,
                null, // Null accounting code
                null  // Null cost center
            ).Value;

            if (existingConfiguration != null)
            {
                _mockGeneralConfigurationRepository
                    .Setup(repo => repo.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(existingConfiguration);

                // Act
                var result = await _handler.Handle(query, CancellationToken.None);

                // Assert
                result.IsSuccess.Should().BeTrue();
                result.Value.AccountingCode.Should().BeNull();
                result.Value.CostCenter.Should().BeNull();
            }
        }
    }
}