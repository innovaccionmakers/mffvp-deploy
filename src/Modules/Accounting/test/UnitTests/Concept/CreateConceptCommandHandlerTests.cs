using Accounting.Application.Abstractions.Data;
using Accounting.Application.Concept.CreateConcept;
using Accounting.Domain.Concepts;
using Accounting.Integrations.Concept.CreateConcept;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Accounting.test.UnitTests.Concept
{
    public class CreateConceptCommandHandlerTests
    {
        private readonly Mock<IConceptsRepository> _mockRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<CreateConceptCommandHandler>> _mockLogger;
        private readonly CreateConceptCommandHandler _handler;

        public CreateConceptCommandHandlerTests()
        {
            _mockRepository = new Mock<IConceptsRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<CreateConceptCommandHandler>>();
            _handler = new CreateConceptCommandHandler(
                _mockRepository.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ReturnsSuccessResult()
        {
            // Arrange
            var command = new CreateConceptCommand(
                PortfolioId: 123,
                Name: "Concepto Test",
                DebitAccount: "DEBIT-123",
                CreditAccount: "CREDIT-456"
            );

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _mockRepository.Verify(repo => repo.Insert(It.IsAny<Domain.Concepts.Concept>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNullAccounts_CreatesConceptSuccessfully()
        {
            // Arrange
            var command = new CreateConceptCommand(
                PortfolioId: 123,
                Name: "Concepto Test",
                DebitAccount: null,
                CreditAccount: null
            );

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _mockRepository.Verify(repo => repo.Insert(It.IsAny<Domain.Concepts.Concept>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

