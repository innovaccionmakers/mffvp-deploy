using Accounting.Application.PassiveTransaction.GetPassiveTransactions;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.PassiveTransaction.GetPassiveTransactions;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Moq;

namespace Accounting.test.UnitTests.PassiveTransaction
{
    public class GetPassiveTransactionsQueryHandlerTests
    {
        private readonly Mock<IPassiveTransactionRepository> _mockRepository;
        private readonly Mock<ILogger<GetPassiveTransactionsQueryHandler>> _mockLogger;
        private readonly IQueryHandler<GetPassiveTransactionsQuery, IReadOnlyCollection<GetPassiveTransactionsResponse>> _handler;

        public GetPassiveTransactionsQueryHandlerTests()
        {
            _mockRepository = new Mock<IPassiveTransactionRepository>();
            _mockLogger = new Mock<ILogger<GetPassiveTransactionsQueryHandler>>();
            _handler = new GetPassiveTransactionsQueryHandler(
                _mockRepository.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Handle_WhenNoPassiveTransactionsExist_ShouldReturnNotFoundResult()
        {
            // Arrange
            var query = new GetPassiveTransactionsQuery();
            var cancellationToken = CancellationToken.None;

            _mockRepository
                .Setup(repo => repo.GetPassiveTransactionsAsync(cancellationToken))
                .ReturnsAsync((List<Domain.PassiveTransactions.PassiveTransaction>)null);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.Error.Type);
            Assert.Equal("No se encontró registro de la configuración contable.", result.Error.Description);

            _mockRepository.Verify(
                repo => repo.GetPassiveTransactionsAsync(cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureResultAndLogError()
        {
            // Arrange
            var query = new GetPassiveTransactionsQuery();
            var cancellationToken = CancellationToken.None;
            var exceptionMessage = "Database connection error";

            _mockRepository
                .Setup(repo => repo.GetPassiveTransactionsAsync(cancellationToken))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.Error.Type);
            Assert.Equal("No hay configuración contable.", result.Error.Description);

            // Verify logging was called
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error al obtener las transacciones pasivas")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenEmptyListReturned_ShouldReturnSuccessResultWithEmptyList()
        {
            // Arrange
            var query = new GetPassiveTransactionsQuery();
            var cancellationToken = CancellationToken.None;

            var emptyList = new List<Domain.PassiveTransactions.PassiveTransaction>();

            _mockRepository
                .Setup(repo => repo.GetPassiveTransactionsAsync(cancellationToken))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);

            _mockRepository.Verify(
                repo => repo.GetPassiveTransactionsAsync(cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectErrorTypeOnException()
        {
            // Arrange
            var query = new GetPassiveTransactionsQuery();
            var cancellationToken = CancellationToken.None;

            _mockRepository
                .Setup(repo => repo.GetPassiveTransactionsAsync(cancellationToken))
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
            var query = new GetPassiveTransactionsQuery();
            var cancellationToken = CancellationToken.None;
            var expectedExceptionMessage = "Test error message";

            _mockRepository
                .Setup(repo => repo.GetPassiveTransactionsAsync(cancellationToken))
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
                        v.ToString().Contains("Error al obtener las transacciones pasivas") &&
                        v.ToString().Contains(expectedExceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}