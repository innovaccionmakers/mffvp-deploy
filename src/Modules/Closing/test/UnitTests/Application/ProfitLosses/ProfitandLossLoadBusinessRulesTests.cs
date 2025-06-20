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

public class ProfitandLossLoadBusinessRulesTests
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

    private static ProfitLossConcept Concept(long id, string name, ProfitLossNature nature, bool allowNegative)
    {
        var concept = ProfitLossConcept.Create(name, nature, allowNegative).Value;
        typeof(ProfitLossConcept)
            .GetProperty(nameof(ProfitLossConcept.ProfitLossConceptId))!
            .SetValue(concept, id);
        return concept;
    }

    [Fact]
    public async Task Handle_Should_Fail_When_NoFutureDate_Rule_Violated()
    {
        // arrange - fecha futura
        var futureDate = DateTime.Today.AddDays(1);
        var conceptAmounts = new Dictionary<string, decimal>
        {
            ["Rendimientos Brutos"] = 100m,
            ["Gastos"] = 50m
        };

        var concepts = new List<ProfitLossConcept>
        {
            Concept(1, "Rendimientos Brutos", ProfitLossNature.Income, true),
            Concept(2, "Gastos", ProfitLossNature.Expense, false)
        };

        _conceptRepo.Setup(r => r.FindByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(concepts);
        _portfolioValidator.Setup(v => v.EnsureExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var ruleError = new RuleValidationError("5063eccb-3107-47a0-8eef-a9775c5c33a6", "La fecha de proceso no puede ser mayor a la fecha actual");
        _ruleEvaluator.Setup(r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { ruleError }));

        var handler = BuildHandler();
        var cmd = new ProfitandLossLoadCommand(10, futureDate, conceptAmounts);

        // act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("5063eccb-3107-47a0-8eef-a9775c5c33a6");
        result.Error.Description.Should().Be("La fecha de proceso no puede ser mayor a la fecha actual");
        _profitLossRepo.Verify(r => r.InsertRange(It.IsAny<IEnumerable<ProfitLoss>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_NegativesOnlyIfAllowed_Rule_Violated()
    {
        // arrange - valor negativo en concepto que no lo permite
        var conceptAmounts = new Dictionary<string, decimal>
        {
            ["Rendimientos Brutos"] = 100m,
            ["Gastos"] = -50m
        };

        var concepts = new List<ProfitLossConcept>
        {
            Concept(1, "Rendimientos Brutos", ProfitLossNature.Income, true),
            Concept(2, "Gastos", ProfitLossNature.Expense, false) // AllowNegative = false
        };

        _conceptRepo.Setup(r => r.FindByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(concepts);
        _portfolioValidator.Setup(v => v.EnsureExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var ruleError = new RuleValidationError("d14d6a61-3fe6-4c07-9f04-35c50a6481d3", "Existen montos negativos en conceptos que no lo permiten");
        _ruleEvaluator.Setup(r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { ruleError }));

        var handler = BuildHandler();
        var cmd = new ProfitandLossLoadCommand(10, DateTime.Today, conceptAmounts);

        // act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("d14d6a61-3fe6-4c07-9f04-35c50a6481d3");
        result.Error.Description.Should().Be("Existen montos negativos en conceptos que no lo permiten");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_AtLeastOneIncome_Rule_Violated()
    {
        // arrange - solo conceptos de gastos, sin ingresos
        var conceptAmounts = new Dictionary<string, decimal>
        {
            ["Gastos Administrativos"] = 50m,
            ["Comisiones"] = 30m
        };

        var concepts = new List<ProfitLossConcept>
        {
            Concept(1, "Gastos Administrativos", ProfitLossNature.Expense, false),
            Concept(2, "Comisiones", ProfitLossNature.Expense, false)
        };

        _conceptRepo.Setup(r => r.FindByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(concepts);
        _portfolioValidator.Setup(v => v.EnsureExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var ruleError = new RuleValidationError("07075653-c27f-484a-8c93-efb1438e4b6b", "Debe existir al menos un concepto de naturaleza Ingreso");
        _ruleEvaluator.Setup(r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { ruleError }));

        var handler = BuildHandler();
        var cmd = new ProfitandLossLoadCommand(10, DateTime.Today, conceptAmounts);

        // act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("07075653-c27f-484a-8c93-efb1438e4b6b");
        result.Error.Description.Should().Be("Debe existir al menos un concepto de naturaleza Ingreso");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_AtLeastOneExpense_Rule_Violated()
    {
        // arrange - solo conceptos de ingresos, sin gastos
        var conceptAmounts = new Dictionary<string, decimal>
        {
            ["Rendimientos Brutos"] = 100m,
            ["Dividendos"] = 80m
        };

        var concepts = new List<ProfitLossConcept>
        {
            Concept(1, "Rendimientos Brutos", ProfitLossNature.Income, true),
            Concept(2, "Dividendos", ProfitLossNature.Income, false)
        };

        _conceptRepo.Setup(r => r.FindByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(concepts);
        _portfolioValidator.Setup(v => v.EnsureExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var ruleError = new RuleValidationError("fc056323-db23-49e5-ab44-1238cf0be8da", "Debe existir al menos un concepto de naturaleza Gasto");
        _ruleEvaluator.Setup(r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { ruleError }));

        var handler = BuildHandler();
        var cmd = new ProfitandLossLoadCommand(10, DateTime.Today, conceptAmounts);

        // act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("fc056323-db23-49e5-ab44-1238cf0be8da");
        result.Error.Description.Should().Be("Debe existir al menos un concepto de naturaleza Gasto");
    }

    [Fact]
    public async Task Handle_Should_Succeed_When_All_Business_Rules_Pass()
    {
        // arrange - caso válido con todas las reglas cumplidas
        var conceptAmounts = new Dictionary<string, decimal>
        {
            ["Rendimientos Brutos"] = 100m,
            ["Gastos Administrativos"] = 30m,
            ["Ajustes"] = -5m
        };

        var concepts = new List<ProfitLossConcept>
        {
            Concept(1, "Rendimientos Brutos", ProfitLossNature.Income, true),
            Concept(2, "Gastos Administrativos", ProfitLossNature.Expense, false),
            Concept(3, "Ajustes", ProfitLossNature.Income, true) // permite negativos
        };

        _conceptRepo.Setup(r => r.FindByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(concepts);
        _portfolioValidator.Setup(v => v.EnsureExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _ruleEvaluator.Setup(r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));

        var handler = BuildHandler();
        var cmd = new ProfitandLossLoadCommand(10, DateTime.Today, conceptAmounts);

        // act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        _profitLossRepo.Verify(r => r.DeleteByPortfolioAndDateAsync(10, DateTime.Today, It.IsAny<CancellationToken>()), Times.Once);
        _profitLossRepo.Verify(r => r.InsertRange(It.Is<IEnumerable<ProfitLoss>>(entries =>
            entries.Count() == 3 &&
            entries.All(e => e.PortfolioId == 10) &&
            entries.All(e => e.EffectiveDate == DateTime.Today) &&
            entries.All(e => e.Source == "Externa")
        )), Times.Once);
        _tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Rollback_Transaction_When_Rules_Fail()
    {
        // arrange
        var conceptAmounts = new Dictionary<string, decimal>
        {
            ["Rendimientos Brutos"] = 100m
        };

        var concepts = new List<ProfitLossConcept>
        {
            Concept(1, "Rendimientos Brutos", ProfitLossNature.Income, true)
        };

        _conceptRepo.Setup(r => r.FindByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(concepts);
        _portfolioValidator.Setup(v => v.EnsureExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var ruleError = new RuleValidationError("TEST", "Test error");
        _ruleEvaluator.Setup(r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { ruleError }));

        var handler = BuildHandler();
        var cmd = new ProfitandLossLoadCommand(10, DateTime.Today, conceptAmounts);

        // act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Pass_Correct_Data_Structure_To_Rules_Engine()
    {
        // arrange
        var effectiveDate = DateTime.Today;
        var conceptAmounts = new Dictionary<string, decimal>
        {
            ["Rendimientos Brutos"] = 100m,
            ["Gastos"] = 50m
        };

        var concepts = new List<ProfitLossConcept>
        {
            Concept(1, "Rendimientos Brutos", ProfitLossNature.Income, true),
            Concept(2, "Gastos", ProfitLossNature.Expense, false)
        };

        object? capturedContext = null;
        _conceptRepo.Setup(r => r.FindByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(concepts);
        _portfolioValidator.Setup(v => v.EnsureExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _ruleEvaluator.Setup(r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Callback<string, object, CancellationToken>((_, context, _) => capturedContext = context)
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));

        var handler = BuildHandler();
        var cmd = new ProfitandLossLoadCommand(10, effectiveDate, conceptAmounts);

        // act
        await handler.Handle(cmd, CancellationToken.None);

        // assert
        capturedContext.Should().NotBeNull();
        var contextType = capturedContext!.GetType();
        
        var effectiveDateProperty = contextType.GetProperty("EffectiveDate");
        effectiveDateProperty.Should().NotBeNull();
        effectiveDateProperty!.GetValue(capturedContext).Should().Be(effectiveDate);

        var conceptsProperty = contextType.GetProperty("Concepts");
        conceptsProperty.Should().NotBeNull();
        var conceptsArray = conceptsProperty!.GetValue(capturedContext) as Array;
        conceptsArray.Should().NotBeNull();
        conceptsArray!.Length.Should().Be(2);
    }
}