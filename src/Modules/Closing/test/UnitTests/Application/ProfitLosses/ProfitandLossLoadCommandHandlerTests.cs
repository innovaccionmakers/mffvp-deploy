using System.Data.Common;
using Closing.Application.Abstractions;
using Closing.Application.Abstractions.Data;
using Closing.Application.Abstractions.External;
using Closing.Application.ProfitLosses.ProfitandLossLoad;
using Closing.Domain.ProfitLossConcepts;
using Closing.Domain.ProfitLosses;
using Closing.Integrations.ProfitLosses.ProfitandLossLoad;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain;
using FluentAssertions;
using Moq;
using RulesEngine.Models;

namespace Closing.test.UnitTests.Application.ProfitLosses;

public class ProfitandLossLoadCommandHandlerTests
{
    private readonly Mock<IProfitLossConceptRepository> _conceptRepo = new();
    private readonly Mock<IProfitLossRepository> _profitLossRepo = new();
    private readonly Mock<IPortfolioValidator> _portfolioValidator = new();
    private readonly Mock<IRuleEvaluator<ClosingModuleMarker>> _ruleEvaluator = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<DbTransaction> _tx = new();

    private ProfitandLossLoadCommandHandler BuildHandler()
    {
        _uow.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_tx.Object);
        return new ProfitandLossLoadCommandHandler(
            _conceptRepo.Object,
            _profitLossRepo.Object,
            _portfolioValidator.Object,
            _ruleEvaluator.Object,
            _uow.Object);
    }

    private static ProfitLossConcept Concept(long id, bool allowNegative)
    {
        var concept = ProfitLossConcept.Create($"c{id}", "N", allowNegative).Value;
        typeof(ProfitLossConcept)
            .GetProperty(nameof(ProfitLossConcept.ProfitLossConceptId))!
            .SetValue(concept, id);
        return concept;
    }

    [Fact]
    public async Task Handle_Should_Return_Success_When_Data_Is_Valid()
    {
        // arrange
        _conceptRepo.Setup(r => r.FindByNameAsync("Rendimientos Brutos", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Concept(1, true));
        _conceptRepo.Setup(r => r.FindByNameAsync("Gastos", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Concept(2, false));
        _portfolioValidator.Setup(v => v.EnsureExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _ruleEvaluator.Setup(r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));
        var handler = BuildHandler();
        var cmd = new ProfitandLossLoadCommand(10, DateTime.Today, 100m, 50m);

        // act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        _profitLossRepo.Verify(r => r.DeleteByPortfolioAndDateAsync(10, DateTime.Today, It.IsAny<CancellationToken>()), Times.Once);
        _profitLossRepo.Verify(r => r.InsertRange(It.Is<IEnumerable<ProfitLoss>>(p => p.Count() == 2)), Times.Once);
        _tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Error_When_Rules_Fail()
    {
        // arrange
        _conceptRepo.Setup(r => r.FindByNameAsync("Rendimientos Brutos", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Concept(1, true));
        _conceptRepo.Setup(r => r.FindByNameAsync("Gastos", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Concept(2, false));
        var ruleErr = new RuleValidationError("C", "bad");
        _portfolioValidator.Setup(v => v.EnsureExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _ruleEvaluator.Setup(r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { ruleErr }));
        var handler = BuildHandler();
        var cmd = new ProfitandLossLoadCommand(10, DateTime.Today, 100m, -10m);

        // act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(ruleErr.Code);
        _profitLossRepo.Verify(r => r.InsertRange(It.IsAny<IEnumerable<ProfitLoss>>()), Times.Never);
    }
}