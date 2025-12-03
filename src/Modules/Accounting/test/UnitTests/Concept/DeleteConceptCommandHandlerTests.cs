using Accounting.Application.Abstractions.Data;
using Accounting.Application.Concept.DeleteConcept;
using Accounting.Domain.Concepts;
using Accounting.Integrations.Concept.DeleteConcept;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Accounting.test.UnitTests.Concept
{
    public class DeleteConceptCommandHandlerTests
    {
        private readonly Mock<IConceptsRepository> _mockRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<DeleteConceptCommandHandler>> _mockLogger;
        private readonly DeleteConceptCommandHandler _handler;

        public DeleteConceptCommandHandlerTests()
        {
            _mockRepository = new Mock<IConceptsRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<DeleteConceptCommandHandler>>();
            _handler = new DeleteConceptCommandHandler(
                _mockRepository.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_WithExistingConcept_DeletesSuccessfully()
        {
            // Arrange
            var conceptId = 1L;
            var command = new DeleteConceptCommand(ConceptId: conceptId);

            var existingConcept = Domain.Concepts.Concept.Create(
                123,
                "Concepto Test",
                "DEBIT-123",
                "CREDIT-456"
            ).Value;

            _mockRepository
                .Setup(repo => repo.GetByIdAsync(conceptId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingConcept);

            var mockTransaction = new Mock<IDbContextTransaction>();
            mockTransaction
                .Setup(tx => tx.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            mockTransaction
                .Setup(tx => tx.DisposeAsync())
                .Returns(ValueTask.CompletedTask);

            _mockUnitOfWork
                .Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockTransaction.Object);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _mockRepository.Verify(repo => repo.GetByIdAsync(conceptId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(repo => repo.Delete(existingConcept), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            mockTransaction.Verify(tx => tx.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistingConcept_ReturnsFailure()
        {
            // Arrange
            var conceptId = 999L;
            var command = new DeleteConceptCommand(ConceptId: conceptId);

            _mockRepository
                .Setup(repo => repo.GetByIdAsync(conceptId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Concepts.Concept)null);

            var mockTransaction = new Mock<IDbContextTransaction>();
            mockTransaction
                .Setup(tx => tx.DisposeAsync())
                .Returns(ValueTask.CompletedTask);

            _mockUnitOfWork
                .Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockTransaction.Object);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(Error.NullValue, result.Error);
            _mockRepository.Verify(repo => repo.GetByIdAsync(conceptId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(repo => repo.Delete(It.IsAny<Domain.Concepts.Concept>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}

