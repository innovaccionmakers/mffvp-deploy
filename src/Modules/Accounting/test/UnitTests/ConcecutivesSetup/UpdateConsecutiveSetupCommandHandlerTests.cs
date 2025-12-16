using Accounting.Application.Abstractions;
using Accounting.Application.ConcecutivesSetup;
using Accounting.Application.Abstractions.Data;
using Accounting.Domain.Consecutives;
using Accounting.Integrations.ConsecutivesSetup;
using Accounting.Infrastructure;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using FluentAssertions;
using Moq;
using RulesEngine.Models;

namespace Accounting.test.UnitTests.ConcecutivesSetup;

public class UpdateConsecutiveSetupCommandHandlerTests
{
    private readonly Mock<IConsecutiveRepository> _consecutiveRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IInternalRuleEvaluator<AccountingModuleMarker>> _ruleEvaluatorMock = new();

    private readonly UpdateConsecutiveSetupCommandHandler _handler;

    public UpdateConsecutiveSetupCommandHandlerTests()
    {
        _handler = new UpdateConsecutiveSetupCommandHandler(
            _consecutiveRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _ruleEvaluatorMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Error_When_Consecutive_Does_Not_Exist()
    {
        // Arrange
        var command = new UpdateConsecutiveSetupCommand(1, "DOC-01", 10);
        var validationError = new RuleValidationError("ACCOUNTING_CONSECUTIVE_001", "El consecutivo solicitado no existe.");

        _consecutiveRepositoryMock
            .Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Consecutive?)null);

        _consecutiveRepositoryMock
            .Setup(r => r.IsSourceDocumentInUseAsync(command.SourceDocument, command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _ruleEvaluatorMock
            .Setup(r => r.EvaluateAsync(
                "Accounting.ConcecutivesSetup.UpdateValidation",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { validationError }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(validationError.Code);
        result.Error.Description.Should().Be(validationError.Message);
        _consecutiveRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Consecutive>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Error_When_SourceDocument_Is_Duplicated()
    {
        // Arrange
        var command = new UpdateConsecutiveSetupCommand(2, "DOC-USED", 20);
        var consecutive = CreateConsecutive(command.Id, "EGR", "OTHER-DOC", 15);
        var validationError = new RuleValidationError("ACCOUNTING_CONSECUTIVE_002", "El documento fuente ya se encuentra asignado a otro consecutivo.");

        _consecutiveRepositoryMock
            .Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(consecutive);

        _consecutiveRepositoryMock
            .Setup(r => r.IsSourceDocumentInUseAsync(command.SourceDocument, command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _ruleEvaluatorMock
            .Setup(r => r.EvaluateAsync(
                "Accounting.ConcecutivesSetup.UpdateValidation",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { validationError }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(validationError.Code);
        result.Error.Description.Should().Be(validationError.Message);
        consecutive.SourceDocument.Should().Be("OTHER-DOC");
        consecutive.Number.Should().Be(15);
        _consecutiveRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Consecutive>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Update_Consecutive_When_Rules_Pass()
    {
        // Arrange
        var command = new UpdateConsecutiveSetupCommand(5, "NEW-DOC", 999);
        var consecutive = CreateConsecutive(command.Id, "ING", "PREV-DOC", 10);

        _consecutiveRepositoryMock
            .Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(consecutive);

        _consecutiveRepositoryMock
            .Setup(r => r.IsSourceDocumentInUseAsync(command.SourceDocument, command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _ruleEvaluatorMock
            .Setup(r => r.EvaluateAsync(
                "Accounting.ConcecutivesSetup.UpdateValidation",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));

        _consecutiveRepositoryMock
            .Setup(r => r.UpdateAsync(consecutive, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new ConsecutiveSetupResponse(
            command.Id,
            "ING",
            command.SourceDocument,
            command.Consecutive));

        consecutive.SourceDocument.Should().Be(command.SourceDocument);
        consecutive.Number.Should().Be(command.Consecutive);

        _consecutiveRepositoryMock.Verify(r => r.UpdateAsync(consecutive, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Consecutive CreateConsecutive(long id, string nature, string sourceDocument, int number)
    {
        var consecutive = Consecutive.Create(nature, sourceDocument, number).Value;
        typeof(Consecutive).GetProperty(nameof(Consecutive.ConsecutiveId))!
            .SetValue(consecutive, id);
        return consecutive;
    }
}
