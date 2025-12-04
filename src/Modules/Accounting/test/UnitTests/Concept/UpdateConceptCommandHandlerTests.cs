using Accounting.Application.Abstractions.Data;
using Accounting.Application.Concept.UpdateConcept;
using Accounting.Domain.Concepts;
using Accounting.Integrations.Concept.UpdateConcept;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Accounting.test.UnitTests.Concept
{
    public class UpdateConceptCommandHandlerTests
    {
        private readonly Mock<IConceptsRepository> _mockRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<UpdateConceptCommandHandler>> _mockLogger;
        private readonly UpdateConceptCommandHandler _handler;

        public UpdateConceptCommandHandlerTests()
        {
            _mockRepository = new Mock<IConceptsRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<UpdateConceptCommandHandler>>();
            _handler = new UpdateConceptCommandHandler(
                _mockRepository.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_WithExistingConcept_UpdatesSuccessfully()
        {
            // Arrange
            var conceptId = 1L;
            var command = new UpdateConceptCommand(
                ConceptId: conceptId,
                DebitAccount: "UPDATED-DEBIT",
                CreditAccount: "UPDATED-CREDIT"
            );

            var existingConcept = Domain.Concepts.Concept.Create(
                123,
                "Concepto Test",
                "OLD-DEBIT",
                "OLD-CREDIT"
            ).Value;

            _mockRepository
                .Setup(repo => repo.GetByIdAsync(conceptId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingConcept);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _mockRepository.Verify(repo => repo.GetByIdAsync(conceptId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(repo => repo.Update(existingConcept), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistingConcept_ReturnsFailure()
        {
            // Arrange
            var conceptId = 999L;
            var command = new UpdateConceptCommand(
                ConceptId: conceptId,
                DebitAccount: "DEBIT-123",
                CreditAccount: "CREDIT-456"
            );

            _mockRepository
                .Setup(repo => repo.GetByIdAsync(conceptId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Concepts.Concept)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(Error.NullValue, result.Error);
            _mockRepository.Verify(repo => repo.GetByIdAsync(conceptId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(repo => repo.Update(It.IsAny<Domain.Concepts.Concept>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}

