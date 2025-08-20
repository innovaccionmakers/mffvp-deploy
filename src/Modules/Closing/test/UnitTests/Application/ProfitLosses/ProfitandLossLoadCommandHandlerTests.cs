using System.Data.Common;
using Closing.Application.Abstractions;
using Closing.Application.Abstractions.Data;
using Closing.Application.Abstractions.External;
using Closing.Application.ProfitLosses.ProfitandLossLoad;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.ProfitLossConcepts;
using Closing.Domain.ProfitLosses;
using Closing.Integrations.ProfitLosses.ProfitandLossLoad;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
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
    private readonly Mock<IPortfolioValuationRepository> _portfolioValuationRepo = new();
    private readonly Mock<IInternalRuleEvaluator<ClosingModuleMarker>> _ruleEvaluator = new();
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
            _portfolioValuationRepo.Object,
            _ruleEvaluator.Object,
            _uow.Object);
    }

    private static ProfitLossConcept Concept(long id, string name, IncomeExpenseNature nature, bool allowNegative)
    {
        var concept = ProfitLossConcept.Create(name, nature, allowNegative).Value;
        typeof(ProfitLossConcept)
            .GetProperty(nameof(ProfitLossConcept.ProfitLossConceptId))!
            .SetValue(concept, id);
        return concept;
    }

    private static ProfitLossConcept Concept(long id, bool allowNegative)
    {
        var concept = ProfitLossConcept.Create($"c{id}", IncomeExpenseNature.Income, allowNegative).Value;
        typeof(ProfitLossConcept)
            .GetProperty(nameof(ProfitLossConcept.ProfitLossConceptId))!
            .SetValue(concept, id);
        return concept;
    }

    [Fact]
    public async Task Handle_Should_Return_Success_When_Data_Is_Valid()
    {
        // arrange
        var conceptAmounts = new Dictionary<string, decimal>
        {
            ["Rendimientos Brutos"] = 100m,
            ["Gastos"] = 50m
        };

        var concepts = new List<ProfitLossConcept>
        {
            Concept(1, "Rendimientos Brutos", IncomeExpenseNature.Income, true),
            Concept(2, "Gastos", IncomeExpenseNature.Expense, false)
        };

        var portfolioData = new PortfolioData(10, DateTime.Today.AddDays(-1));

        _conceptRepo.Setup(r => r.FindByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(concepts);
        _portfolioValidator.Setup(v => v.GetPortfolioDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(portfolioData));
        _portfolioValuationRepo.Setup(r => r.ExistsByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _ruleEvaluator.Setup(r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));
        var handler = BuildHandler();
        var cmd = new ProfitandLossLoadCommand(10, DateTime.Today, conceptAmounts);

        // act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        _profitLossRepo.Verify(r => r.DeleteByPortfolioAndDateAsync(10, DateTime.Today, It.IsAny<CancellationToken>()), Times.Once);
        _profitLossRepo.Verify(r => r.InsertRange(It.IsAny<IEnumerable<ProfitLoss>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Rules_Fail()
    {
        // arrange
        var conceptAmounts = new Dictionary<string, decimal>
        {
            ["Rendimientos Brutos"] = 100m
        };

        var concepts = new List<ProfitLossConcept>
        {
            Concept(1, "Rendimientos Brutos", IncomeExpenseNature.Income, true)
        };

        var portfolioData = new PortfolioData(10, DateTime.Today.AddDays(-1));
        var validationError = new RuleValidationError("CLOSING_001", "La fecha de proceso no puede ser mayor a la fecha actual");

        _conceptRepo.Setup(r => r.FindByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(concepts);
        _portfolioValidator.Setup(v => v.GetPortfolioDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(portfolioData));
        _portfolioValuationRepo.Setup(r => r.ExistsByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _ruleEvaluator.Setup(r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { validationError }));
        var handler = BuildHandler();
        var cmd = new ProfitandLossLoadCommand(10, DateTime.Today.AddDays(1), conceptAmounts);

        // act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("CLOSING_001");
        result.Error.Description.Should().Be("La fecha de proceso no puede ser mayor a la fecha actual");
        _profitLossRepo.Verify(r => r.DeleteByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        _profitLossRepo.Verify(r => r.InsertRange(It.IsAny<IEnumerable<ProfitLoss>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Portfolio_Does_Not_Exist()
    {
        // arrange
        var conceptAmounts = new Dictionary<string, decimal>
        {
            ["Rendimientos Brutos"] = 100m
        };

        _portfolioValidator.Setup(v => v.GetPortfolioDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<PortfolioData>(Error.NotFound("Portfolio.NotFound", "El portafolio no existe")));
        var handler = BuildHandler();
        var cmd = new ProfitandLossLoadCommand(10, DateTime.Today, conceptAmounts);

        // act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Portfolio.NotFound");
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Concepts_Do_Not_Exist()
    {
        // arrange
        var conceptAmounts = new Dictionary<string, decimal>
        {
            ["ConceptoInexistente"] = 100m,
            ["OtroConceptoInexistente"] = 50m
        };

        var portfolioData = new PortfolioData(10, DateTime.Today.AddDays(-1));
        var validationError = new RuleValidationError("CLOSING_000", "No existen algunos de los conceptos solicitados");

        _conceptRepo.Setup(r => r.FindByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProfitLossConcept>()); // No se encuentran conceptos
        _portfolioValidator.Setup(v => v.GetPortfolioDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(portfolioData));
        _portfolioValuationRepo.Setup(r => r.ExistsByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _ruleEvaluator.Setup(r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { validationError }));
        var handler = BuildHandler();
        var cmd = new ProfitandLossLoadCommand(10, DateTime.Today, conceptAmounts);

        // act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("CLOSING_000");
        result.Error.Description.Should().Be("No existen algunos de los conceptos solicitados");
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Effective_Date_Is_Not_Next_Day()
    {
        // arrange
        var conceptAmounts = new Dictionary<string, decimal>
        {
            ["Rendimientos Brutos"] = 100m,
            ["Gastos"] = 50m
        };

        var concepts = new List<ProfitLossConcept>
        {
            Concept(1, "Rendimientos Brutos", IncomeExpenseNature.Income, true),
            Concept(2, "Gastos", IncomeExpenseNature.Expense, false)
        };

        var portfolioCurrentDate = new DateTime(2025, 7, 1);
        var portfolioData = new PortfolioData(10, portfolioCurrentDate);
        var invalidEffectiveDate = new DateTime(2025, 7, 3); // Dos días después en lugar de uno
        var validationError = new RuleValidationError("CLOSING_005", "La fecha efectiva debe ser exactamente un día después de la fecha actual del portafolio");

        _conceptRepo.Setup(r => r.FindByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(concepts);
        _portfolioValidator.Setup(v => v.GetPortfolioDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(portfolioData));
        _portfolioValuationRepo.Setup(r => r.ExistsByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _ruleEvaluator.Setup(r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { validationError }));
        var handler = BuildHandler();
        var cmd = new ProfitandLossLoadCommand(10, invalidEffectiveDate, conceptAmounts);

        // act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("CLOSING_005");
        result.Error.Description.Should().Be("La fecha efectiva debe ser exactamente un día después de la fecha actual del portafolio");
        _profitLossRepo.Verify(r => r.DeleteByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        _profitLossRepo.Verify(r => r.InsertRange(It.IsAny<IEnumerable<ProfitLoss>>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_Should_Return_Failure_When_Portfolio_Valuation_Does_Not_Exist()
    {
        // arrange
        var conceptAmounts = new Dictionary<string, decimal>
        {
            ["Rendimientos Brutos"] = 100m
        };

        var concepts = new List<ProfitLossConcept>
        {
            Concept(1, "Rendimientos Brutos", IncomeExpenseNature.Income, true)
        };

        var portfolioData = new PortfolioData(10, DateTime.Today.AddDays(-1));
        var validationError = new RuleValidationError("CLOSING_004", "Debe existir una valoración para la fecha actual del portafolio");

        _conceptRepo.Setup(r => r.FindByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(concepts);
        _portfolioValidator.Setup(v => v.GetPortfolioDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(portfolioData));
        _portfolioValuationRepo.Setup(r => r.ExistsByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _ruleEvaluator.Setup(r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { validationError }));
        var handler = BuildHandler();
        var cmd = new ProfitandLossLoadCommand(10, DateTime.Today, conceptAmounts);

        // act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("CLOSING_004");
        result.Error.Description.Should().Be("Debe existir una valoración para la fecha actual del portafolio");
    }

    [Fact]
    public async Task Handle_Should_Use_Correct_Workflow_Name_For_Rules()
    {
        // arrange
        var conceptAmounts = new Dictionary<string, decimal>
        {
            ["Rendimientos Brutos"] = 100m
        };

        var concepts = new List<ProfitLossConcept>
        {
            Concept(1, "Rendimientos Brutos", IncomeExpenseNature.Income, true)
        };

        var portfolioData = new PortfolioData(10, DateTime.Today.AddDays(-1));

        _conceptRepo.Setup(r => r.FindByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(concepts);
        _portfolioValidator.Setup(v => v.GetPortfolioDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(portfolioData));
        _portfolioValuationRepo.Setup(r => r.ExistsByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _ruleEvaluator.Setup(r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));
        var handler = BuildHandler();
        var cmd = new ProfitandLossLoadCommand(10, DateTime.Today, conceptAmounts);

        // act
        await handler.Handle(cmd, CancellationToken.None);

        // assert
        _ruleEvaluator.Verify(r => r.EvaluateAsync(
            "Closing.ProfitLoss.UploadValidationV2",
            It.IsAny<object>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Create_ProfitLoss_Entries_With_Correct_Data()
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
            Concept(1, "Rendimientos Brutos", IncomeExpenseNature.Income, true),
            Concept(2, "Gastos", IncomeExpenseNature.Expense, false)
        };

        var portfolioData = new PortfolioData(10, DateTime.Today.AddDays(-1));

        _conceptRepo.Setup(r => r.FindByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(concepts);
        _portfolioValidator.Setup(v => v.GetPortfolioDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(portfolioData));
        _portfolioValuationRepo.Setup(r => r.ExistsByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _ruleEvaluator.Setup(r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));
        var handler = BuildHandler();
        var cmd = new ProfitandLossLoadCommand(10, effectiveDate, conceptAmounts);

        // act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        _profitLossRepo.Verify(r => r.InsertRange(It.IsAny<IEnumerable<ProfitLoss>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Pass_Correct_Context_To_Rules_Evaluator_Including_Portfolio_Current_Date()
    {
        // arrange
        var effectiveDate = DateTime.Today;
        var portfolioCurrentDate = DateTime.Today.AddDays(-1);
        var conceptAmounts = new Dictionary<string, decimal>
        {
            ["Rendimientos Brutos"] = 100m,
            ["Gastos"] = 50m
        };

        var concepts = new List<ProfitLossConcept>
        {
            Concept(1, "Rendimientos Brutos", IncomeExpenseNature.Income, true),
            Concept(2, "Gastos", IncomeExpenseNature.Expense, false)
        };

        var portfolioData = new PortfolioData(10, portfolioCurrentDate);

        _conceptRepo.Setup(r => r.FindByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(concepts);
        _portfolioValidator.Setup(v => v.GetPortfolioDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(portfolioData));
        _portfolioValuationRepo.Setup(r => r.ExistsByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _ruleEvaluator.Setup(r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));
        var handler = BuildHandler();
        var cmd = new ProfitandLossLoadCommand(10, effectiveDate, conceptAmounts);

        // act
        await handler.Handle(cmd, CancellationToken.None);

        // assert
        _ruleEvaluator.Verify(r => r.EvaluateAsync(
            "Closing.ProfitLoss.UploadValidationV2",
            It.Is<object>(context => ValidateRuleContextWithPortfolioCurrentDate(context, effectiveDate, portfolioCurrentDate, concepts, conceptAmounts)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static bool ValidateRuleContextWithPortfolioCurrentDate(object context, DateTime expectedEffectiveDate, DateTime expectedPortfolioCurrentDate, List<ProfitLossConcept> expectedConcepts, Dictionary<string, decimal> expectedAmounts)
    {
        var contextType = context.GetType();
        var effectiveDateProperty = contextType.GetProperty("EffectiveDate");
        var portfolioCurrentDateProperty = contextType.GetProperty("PortfolioCurrentDate");
        var portfolioValuationExistsProperty = contextType.GetProperty("PortfolioValuationExists");
        var conceptsProperty = contextType.GetProperty("Concepts");
        var requestedConceptNamesProperty = contextType.GetProperty("RequestedConceptNames");

        if (effectiveDateProperty == null || portfolioCurrentDateProperty == null || portfolioValuationExistsProperty == null || conceptsProperty == null || requestedConceptNamesProperty == null)
            return false;

        var effectiveDate = (DateTime)effectiveDateProperty.GetValue(context)!;
        var portfolioCurrentDate = (DateTime)portfolioCurrentDateProperty.GetValue(context)!;
        var valuationExists = (bool)portfolioValuationExistsProperty.GetValue(context)!;
        var concepts = conceptsProperty.GetValue(context) as Array;
        var requestedConceptNames = requestedConceptNamesProperty.GetValue(context) as Array;
        
        if (effectiveDate != expectedEffectiveDate || portfolioCurrentDate != expectedPortfolioCurrentDate || !valuationExists || concepts == null || requestedConceptNames == null)
            return false;

        if (concepts.Length != expectedConcepts.Count)
            return false;

        if (requestedConceptNames.Length != expectedAmounts.Keys.Count)
            return false;

        for (int i = 0; i < concepts.Length; i++)
        {
            var concept = concepts.GetValue(i);
            var conceptType = concept!.GetType();
            var conceptNameProperty = conceptType.GetProperty("Concept");
            var amountProperty = conceptType.GetProperty("Amount");
            var isIncomeProperty = conceptType.GetProperty("IsIncome");
            var isExpenseProperty = conceptType.GetProperty("IsExpense");

            if (conceptNameProperty == null || amountProperty == null || isIncomeProperty == null || isExpenseProperty == null)
                return false;

            var conceptName = (string)conceptNameProperty.GetValue(concept)!;
            var amount = (decimal)amountProperty.GetValue(concept)!;
            var isIncome = (bool)isIncomeProperty.GetValue(concept)!;
            var isExpense = (bool)isExpenseProperty.GetValue(concept)!;

            var expectedConcept = expectedConcepts.First(c => c.Concept == conceptName);
            var expectedIsIncome = expectedConcept.Nature == IncomeExpenseNature.Income;
            var expectedIsExpense = expectedConcept.Nature == IncomeExpenseNature.Expense;

            if (!expectedAmounts.ContainsKey(conceptName) ||
                expectedAmounts[conceptName] != amount ||
                isIncome != expectedIsIncome ||
                isExpense != expectedIsExpense)
                return false;
        }

        return true;
    }

    private static bool ValidateRuleContext(object context, DateTime expectedDate, List<ProfitLossConcept> expectedConcepts, Dictionary<string, decimal> expectedAmounts)
    {
        var contextType = context.GetType();
        var effectiveDateProperty = contextType.GetProperty("EffectiveDate");
        var conceptsProperty = contextType.GetProperty("Concepts");
        var requestedConceptNamesProperty = contextType.GetProperty("RequestedConceptNames");

        if (effectiveDateProperty == null || conceptsProperty == null || requestedConceptNamesProperty == null)
            return false;

        var effectiveDate = (DateTime)effectiveDateProperty.GetValue(context)!;
        var concepts = conceptsProperty.GetValue(context) as Array;
        var requestedConceptNames = requestedConceptNamesProperty.GetValue(context) as Array;

        if (effectiveDate != expectedDate || concepts == null || requestedConceptNames == null)
            return false;

        if (concepts.Length != expectedConcepts.Count)
            return false;

        if (requestedConceptNames.Length != expectedAmounts.Keys.Count)
            return false;

        for (int i = 0; i < concepts.Length; i++)
        {
            var concept = concepts.GetValue(i);
            var conceptType = concept!.GetType();
            var conceptNameProperty = conceptType.GetProperty("Concept");
            var amountProperty = conceptType.GetProperty("Amount");
            var isIncomeProperty = conceptType.GetProperty("IsIncome");
            var isExpenseProperty = conceptType.GetProperty("IsExpense");

            if (conceptNameProperty == null || amountProperty == null || isIncomeProperty == null || isExpenseProperty == null)
                return false;

            var conceptName = (string)conceptNameProperty.GetValue(concept)!;
            var amount = (decimal)amountProperty.GetValue(concept)!;
            var isIncome = (bool)isIncomeProperty.GetValue(concept)!;
            var isExpense = (bool)isExpenseProperty.GetValue(concept)!;

            var expectedConcept = expectedConcepts.First(c => c.Concept == conceptName);
            var expectedIsIncome = expectedConcept.Nature == IncomeExpenseNature.Income;
            var expectedIsExpense = expectedConcept.Nature == IncomeExpenseNature.Expense;

            if (!expectedAmounts.ContainsKey(conceptName) ||
                expectedAmounts[conceptName] != amount ||
                isIncome != expectedIsIncome ||
                isExpense != expectedIsExpense)
                return false;
        }

        return true;
    }
}