using Accounting.Application.Abstractions.Data;
using Accounting.Application.ConfigurationGenerals.CreateConfigurationGeneral;
using Accounting.Domain.ConfigurationGenerals;
using Accounting.Integrations.ConfigurationGenerals.CreateConfigurationGeneral;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Accounting.test.UnitTests.ConfigurationGenerals
{
    public class CreateConfigurationGeneralCommandHandlerTests
    {
        private readonly Mock<IGeneralConfigurationRepository> _mockGeneralConfigurationRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<CreateConfigurationGeneralCommandHandler>> _mockLogger;
        private readonly CreateConfigurationGeneralCommandHandler _handler;

        public CreateConfigurationGeneralCommandHandlerTests()
        {
            _mockGeneralConfigurationRepository = new Mock<IGeneralConfigurationRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<CreateConfigurationGeneralCommandHandler>>();

            _handler = new CreateConfigurationGeneralCommandHandler(
                _mockGeneralConfigurationRepository.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldCreateConfigurationGeneral_WhenNoExistingConfiguration()
        {
            // Arrange
            var portfolioId = 1;
            var command = new CreateConfigurationGeneralCommand(
                portfolioId,
                "ACC001",
                "CC001"
            );

            _mockGeneralConfigurationRepository
                .Setup(repo => repo.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((GeneralConfiguration)null);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();

            _mockGeneralConfigurationRepository.Verify(
                repo => repo.Insert(It.IsAny<GeneralConfiguration>()),
                Times.Once
            );

            _mockUnitOfWork.Verify(
                uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenConfigurationAlreadyExists()
        {
            // Arrange
            var portfolioId = 1;
            var command = new CreateConfigurationGeneralCommand(
                portfolioId,
                "ACC001",
                "CC001"
            );

            var existingConfiguration = GeneralConfiguration.Create(
                portfolioId,
                "ACC002",
                "CC002"
            ).Value;

            _mockGeneralConfigurationRepository
                .Setup(repo => repo.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingConfiguration);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("0");
            result.Error.Description.Should().Contain("Ya se encuentra creada una configuración general con ese portafolio");

            _mockGeneralConfigurationRepository.Verify(
                repo => repo.Insert(It.IsAny<GeneralConfiguration>()),
                Times.Never
            );

            _mockUnitOfWork.Verify(
                uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Fact]
        public async Task Handle_ShouldCallRepositoryWithCorrectParameters()
        {
            // Arrange
            var portfolioId = 1;
            var accountingCode = "ACC001";
            var costCenter = "CC001";

            var command = new CreateConfigurationGeneralCommand(
                portfolioId,
                accountingCode,
                costCenter
            );

            _mockGeneralConfigurationRepository
                .Setup(repo => repo.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((GeneralConfiguration)null);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            GeneralConfiguration capturedConfiguration = null;
            _mockGeneralConfigurationRepository
                .Setup(repo => repo.Insert(It.IsAny<GeneralConfiguration>()))
                .Callback<GeneralConfiguration>(config => capturedConfiguration = config);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            capturedConfiguration.Should().NotBeNull();
            capturedConfiguration.PortfolioId.Should().Be(portfolioId);

            // Note: Depending on your GeneralConfiguration implementation,
            // you might need to expose properties or use reflection to verify values
            // This assumes GeneralConfiguration has these properties accessible
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenSaveChangesReturnsZero()
        {
            // Arrange
            var portfolioId = 1;
            var command = new CreateConfigurationGeneralCommand(
                portfolioId,
                "ACC001",
                "CC001"
            );

            _mockGeneralConfigurationRepository
                .Setup(repo => repo.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((GeneralConfiguration)null);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0); // SaveChanges returns 0

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();

            _mockGeneralConfigurationRepository.Verify(
                repo => repo.Insert(It.IsAny<GeneralConfiguration>()),
                Times.Once
            );

            _mockUnitOfWork.Verify(
                uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }
}
