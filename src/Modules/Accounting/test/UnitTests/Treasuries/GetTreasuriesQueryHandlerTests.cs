using Accounting.Application.Treasuries.GetTreasuries;
using Accounting.Domain.Treasuries;
using Accounting.Integrations.Treasuries.GetTreasuries;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Moq;

namespace Accounting.test.UnitTests.Treasuries
{
    public class GetTreasuriesQueryHandlerTests
    {
        private readonly Mock<ITreasuryRepository> _mockRepository;
        private readonly Mock<ILogger<GetTreasuriesQueryHandler>> _mockLogger;
        private readonly GetTreasuriesQueryHandler _handler;

        public GetTreasuriesQueryHandlerTests()
        {
            _mockRepository = new Mock<ITreasuryRepository>();
            _mockLogger = new Mock<ILogger<GetTreasuriesQueryHandler>>();
            _handler = new GetTreasuriesQueryHandler(
                _mockRepository.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureResultAndLogError()
        {
            // Arrange
            var query = new GetTreasuriesQuery();
            var cancellationToken = CancellationToken.None;
            var exceptionMessage = "Database error occurred";

            _mockRepository
                .Setup(repo => repo.GetTreasuriesAsync(cancellationToken))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.Error.Type);
            Assert.Equal("No hay registros de tesorería", result.Error.Description);

            // Verify logging was called
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error al obtener tesorería")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectErrorTypeOnException()
        {
            // Arrange
            var query = new GetTreasuriesQuery();
            var cancellationToken = CancellationToken.None;

            _mockRepository
                .Setup(repo => repo.GetTreasuriesAsync(cancellationToken))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.Error.Type);
        }

        [Fact]
        public async Task Handle_ShouldLogCorrectErrorMessageOnException()
        {
            // Arrange
            var query = new GetTreasuriesQuery();
            var cancellationToken = CancellationToken.None;
            var expectedExceptionMessage = "Specific error message";

            _mockRepository
                .Setup(repo => repo.GetTreasuriesAsync(cancellationToken))
                .ThrowsAsync(new Exception(expectedExceptionMessage));

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            // Verify that the exception message is logged
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Error al obtener tesorería") &&
                        v.ToString().Contains(expectedExceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}