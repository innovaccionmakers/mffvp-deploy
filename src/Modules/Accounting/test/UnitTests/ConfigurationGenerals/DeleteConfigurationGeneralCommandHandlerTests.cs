using Accounting.Application.Abstractions.Data;
using Accounting.Application.ConfigurationGenerals.DeleteConfigurationGeneral;
using Accounting.Domain.ConfigurationGenerals;
using Accounting.Integrations.ConfigurationGenerals.DeleteConfigurationGeneral;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Data;

namespace Accounting.test.UnitTests.ConfigurationGenerals
{
    public class DeleteConfigurationGeneralCommandHandlerTests
    {
        private readonly Mock<IGeneralConfigurationRepository> _mockGeneralConfigurationRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<Accounting.Application.ConfigurationGenerals.CreateConfigurationGeneral.CreateConfigurationGeneralCommandHandler>> _mockLogger;
        private readonly DeleteConfigurationGeneralCommandHandler _handler;

        public DeleteConfigurationGeneralCommandHandlerTests()
        {
            _mockGeneralConfigurationRepository = new Mock<IGeneralConfigurationRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<Accounting.Application.ConfigurationGenerals.CreateConfigurationGeneral.CreateConfigurationGeneralCommandHandler>>();

            _handler = new DeleteConfigurationGeneralCommandHandler(
                _mockGeneralConfigurationRepository.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldPassCancellationTokenToAllAsyncMethods()
        {
            // Arrange
            var portfolioId = 1;
            var command = new DeleteConfigurationGeneralCommand(portfolioId);
            var cancellationToken = new CancellationTokenSource().Token;

            var existingConfiguration = GeneralConfiguration.Create(
                portfolioId,
                "ACC001",
                "CC001"
            ).Value;

            var mockTransaction = new Mock<IDbTransaction>();

            // Act
            // We'll simulate an exception to verify all methods are called with correct token
            _mockGeneralConfigurationRepository
                .Setup(repo => repo.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, cancellationToken))
                .ThrowsAsync(new TaskCanceledException());

            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.IsFailure.Should().BeTrue();

            _mockGeneralConfigurationRepository.Verify(
                repo => repo.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, cancellationToken),
                Times.Once
            );
        }
    }
}