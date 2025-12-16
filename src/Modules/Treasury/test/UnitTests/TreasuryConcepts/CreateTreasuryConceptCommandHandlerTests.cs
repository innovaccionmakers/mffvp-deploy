using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain;
using FluentAssertions;
using Moq;
using Treasury.Application.Abstractions;
using Treasury.Application.Abstractions.Data;
using Treasury.Application.TreasuryConcepts.Commands;
using Treasury.Domain.TreasuryConcepts;
using Treasury.Integrations.TreasuryConcepts.Commands;

namespace Treasury.test.UnitTests.TreasuryConcepts
{
    public class CreateTreasuryConceptCommandHandlerTests
    {
        private readonly Mock<ITreasuryConceptRepository> _mockRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IInternalRuleEvaluator<TreasuryModuleMarker>> _mockRuleEvaluator;
        private readonly CreateTreasuryConceptCommandHandler _handler;

        public CreateTreasuryConceptCommandHandlerTests()
        {
            _mockRepository = new Mock<ITreasuryConceptRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockRuleEvaluator = new Mock<IInternalRuleEvaluator<TreasuryModuleMarker>>();

            _handler = new CreateTreasuryConceptCommandHandler(
                _mockRepository.Object,
                _mockUnitOfWork.Object,
                _mockRuleEvaluator.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenConceptAlreadyExists()
        {
            // Arrange
            var command = new CreateTreasuryConceptCommand(
                "EXIST001",
                IncomeExpenseNature.Income,
                false,
                true,
                false,
                true,
                null
            );

            _mockRepository
                .Setup(repo => repo.GetByConceptAsync(command.Concept, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("0");
            result.Error.Description.Should().Contain("El código del concepto ya existe");

            _mockRuleEvaluator.Verify(
                evaluator => evaluator.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
                Times.Never
            );

            _mockRepository.Verify(
                repo => repo.AddAsync(It.IsAny<TreasuryConcept>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }
    }
}