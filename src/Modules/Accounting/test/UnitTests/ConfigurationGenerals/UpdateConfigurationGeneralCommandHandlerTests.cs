using Accounting.Application.Abstractions.Data;
using Accounting.Application.ConfigurationGenerals.UpdateConfigurationGeneral;
using Accounting.Domain.ConfigurationGenerals;
using Accounting.Integrations.ConfigurationGenerals.UpdateConfigurationGeneral;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Accounting.test.UnitTests.ConfigurationGenerals
{
    public class UpdateConfigurationGeneralCommandHandlerTests
    {
        private readonly Mock<IGeneralConfigurationRepository> _mockGeneralConfigurationRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<Accounting.Application.ConfigurationGenerals.CreateConfigurationGeneral.CreateConfigurationGeneralCommandHandler>> _mockLogger;
        private readonly UpdateConfigurationGeneralCommandHandler _handler;

        public UpdateConfigurationGeneralCommandHandlerTests()
        {
            _mockGeneralConfigurationRepository = new Mock<IGeneralConfigurationRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<Accounting.Application.ConfigurationGenerals.CreateConfigurationGeneral.CreateConfigurationGeneralCommandHandler>>();

            _handler = new UpdateConfigurationGeneralCommandHandler(
                _mockGeneralConfigurationRepository.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldUpdateConfigurationGeneral_WhenConfigurationExists()
        {
            // Arrange
            var portfolioId = 1;
            var newAccountingCode = "NEW_ACC001";
            var newCostCenter = "NEW_CC001";

            var command = new UpdateConfigurationGeneralCommand(
                portfolioId,
                newAccountingCode,
                newCostCenter
            );

            var existingConfiguration = GeneralConfiguration.Create(
                portfolioId,
                "OLD_ACC001",
                "OLD_CC001"
            ).Value;

            _mockGeneralConfigurationRepository
                .Setup(repo => repo.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingConfiguration);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();

            _mockGeneralConfigurationRepository.Verify(
                repo => repo.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, It.IsAny<CancellationToken>()),
                Times.Once
            );

            _mockGeneralConfigurationRepository.Verify(
                repo => repo.Update(existingConfiguration),
                Times.Once
            );

            _mockUnitOfWork.Verify(
                uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );

            // Verify that UpdateDetails was called (we can verify this by checking the state if possible)
            // This assumes UpdateDetails modifies the object's state
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenSaveChangesReturnsZero()
        {
            // Arrange
            var portfolioId = 1;
            var command = new UpdateConfigurationGeneralCommand(
                portfolioId,
                "ACC001",
                "CC001"
            );

            var existingConfiguration = GeneralConfiguration.Create(
                portfolioId,
                "OLD_ACC",
                "OLD_CC"
            ).Value;

            _mockGeneralConfigurationRepository
                .Setup(repo => repo.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingConfiguration);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0); // SaveChanges returns 0

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();

            _mockGeneralConfigurationRepository.Verify(
                repo => repo.Update(existingConfiguration),
                Times.Once
            );

            _mockUnitOfWork.Verify(
                uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_ShouldPassCancellationTokenToAllAsyncMethods()
        {
            // Arrange
            var portfolioId = 1;
            var command = new UpdateConfigurationGeneralCommand(
                portfolioId,
                "ACC001",
                "CC001"
            );

            var cancellationToken = new CancellationTokenSource().Token;

            var existingConfiguration = GeneralConfiguration.Create(
                portfolioId,
                "OLD_ACC",
                "OLD_CC"
            ).Value;

            _mockGeneralConfigurationRepository
                .Setup(repo => repo.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, cancellationToken))
                .ReturnsAsync(existingConfiguration);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.IsSuccess.Should().BeTrue();

            _mockGeneralConfigurationRepository.Verify(
                repo => repo.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, cancellationToken),
                Times.Once
            );

            _mockUnitOfWork.Verify(
                uow => uow.SaveChangesAsync(cancellationToken),
                Times.Once
            );
        }
    }
}