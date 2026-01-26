using Accounting.Application.ConfigurationGenerals.GetConfigurationsGeneral;
using Accounting.Domain.ConfigurationGenerals;
using Accounting.Integrations.ConfigurationGenerals.GetConfigurationGenerals;
using Common.SharedKernel.Core.Primitives;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace Accounting.test.UnitTests.ConfigurationGenerals
{
    public class GetConfigurationGeneralsQueryHandlerTests
    {
        private readonly Mock<IGeneralConfigurationRepository> _mockGeneralConfigurationRepository;
        private readonly Mock<ILogger<GetConfigurationGeneralsQueryHandler>> _mockLogger;
        private readonly GetConfigurationGeneralsQueryHandler _handler;

        public GetConfigurationGeneralsQueryHandlerTests()
        {
            _mockGeneralConfigurationRepository = new Mock<IGeneralConfigurationRepository>();
            _mockLogger = new Mock<ILogger<GetConfigurationGeneralsQueryHandler>>();
            _handler = new GetConfigurationGeneralsQueryHandler(
                _mockGeneralConfigurationRepository.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureResultAndLogError()
        {
            // Arrange
            var query = new GetConfigurationGeneralsQuery();
            var cancellationToken = CancellationToken.None;
            var exceptionMessage = "Database connection failed";

            _mockGeneralConfigurationRepository
                .Setup(repo => repo.GetConfigurationGeneralsAsync(cancellationToken))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.Error.Type);
            Assert.Equal("No se pudo obtener la configuración general.", result.Error.Description);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error al consultar la configuración general")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}