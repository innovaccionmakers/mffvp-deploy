using Microsoft.Extensions.Logging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Infrastructure.RulesEngine;
using Closing.Domain.ProfitLossConcepts;
using FluentAssertions;
using Moq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Closing.Application.Abstractions;
using Closing.Infrastructure;

namespace Closing.test.UnitTests.Application.ProfitLosses;

public class InternalRuleEvaluatorIntegrationTests
{
    private readonly IServiceProvider _serviceProvider;

    public InternalRuleEvaluatorIntegrationTests()
    {
        var services = new ServiceCollection();

        services.AddRulesEngine<ClosingModuleMarker>(typeof(ClosingModule).Assembly, opt =>
        {
            opt.CacheSizeLimitMb = 64;
            opt.EmbeddedResourceSearchPatterns = [".rules.json"];
        });

        services.AddLogging(builder => builder.AddConsole());

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task InternalRuleEvaluator_Should_Return_Internal_Code_From_Rules_Json()
    {
        // Arrange
        var evaluator = _serviceProvider.GetRequiredService<IInternalRuleEvaluator<ClosingModuleMarker>>();

        var conceptNames = new[] { "Rendimientos Brutos", "Gastos" };
        var ruleContext = new
        {
            EffectiveDate = DateTime.UtcNow.AddDays(1),
            PortfolioCurrentDate = DateTime.UtcNow.Date,
            RequestedConceptNames = conceptNames,
            Concepts = new[]
            {
                new
                {
                    ProfitLossConceptId = 1L,
                    Concept = "Rendimientos Brutos",
                    Nature = ProfitLossNature.Income,
                    AllowNegative = true,
                    Amount = 100m,
                    IsIncome = true,
                    IsExpense = false
                },
                new
                {
                    ProfitLossConceptId = 2L,
                    Concept = "Gastos",
                    Nature = ProfitLossNature.Expense,
                    AllowNegative = false,
                    Amount = 50m,
                    IsIncome = false,
                    IsExpense = true
                }
            }
        };

        // Act
        var (isValid, _, validationErrors) = await evaluator
            .EvaluateAsync("Closing.ProfitLoss.UploadValidationV2", ruleContext);

        // Assert
        isValid.Should().BeFalse();
        validationErrors.Should().HaveCount(1);

        var error = validationErrors.First();
        error.Code.Should().Be("CLOSING_001");
        error.Message.Should().Be("La fecha de proceso no puede ser mayor a la fecha actual");
    }

    [Fact]
    public async Task InternalRuleEvaluator_Should_Validate_Negative_Amounts_Rule()
    {
        // Arrange
        var evaluator = _serviceProvider.GetRequiredService<IInternalRuleEvaluator<ClosingModuleMarker>>();

        var conceptNames = new[] { "Rendimientos Brutos", "Gastos" };
        var ruleContext = new
        {
            EffectiveDate = DateTime.UtcNow.Date,
            PortfolioCurrentDate = DateTime.UtcNow.Date.AddDays(-1),
            RequestedConceptNames = conceptNames,
            Concepts = new[]
            {
                new
                {
                    ProfitLossConceptId = 1L,
                    Concept = "Rendimientos Brutos",
                    Nature = ProfitLossNature.Income,
                    AllowNegative = true,
                    Amount = 100m,
                    IsIncome = true,
                    IsExpense = false
                },
                new
                {
                    ProfitLossConceptId = 2L,
                    Concept = "Gastos",
                    Nature = ProfitLossNature.Expense,
                    AllowNegative = false,
                    Amount = -50m,
                    IsIncome = false,
                    IsExpense = true
                }
            }
        };

        // Act
        var (isValid, _, validationErrors) = await evaluator
            .EvaluateAsync("Closing.ProfitLoss.UploadValidationV2", ruleContext);

        // Assert
        isValid.Should().BeFalse();
        validationErrors.Should().HaveCount(1);

        var error = validationErrors.First();
        error.Code.Should().Be("CLOSING_002");
        error.Message.Should().Be("Existen montos negativos en conceptos que no lo permiten");
    }

    [Fact]
    public async Task InternalRuleEvaluator_Should_Validate_All_Concepts_Exist_Rule()
    {
        // Arrange
        var evaluator = _serviceProvider.GetRequiredService<IInternalRuleEvaluator<ClosingModuleMarker>>();

        var conceptNames = new[] { "Concepto Inexistente", "Otro Concepto", "Concepto Válido" };
        var ruleContext = new
        {
            EffectiveDate = DateTime.UtcNow.Date,
            PortfolioCurrentDate = DateTime.UtcNow.Date.AddDays(-1),
            RequestedConceptNames = conceptNames,
            Concepts = new[]
            {
                new
                {
                    ProfitLossConceptId = 1L,
                    Concept = "Concepto Válido",
                    Nature = ProfitLossNature.Income,
                    AllowNegative = true,
                    Amount = 100m,
                    IsIncome = true,
                    IsExpense = false
                }
            }
        };

        // Act
        var (isValid, _, validationErrors) = await evaluator
            .EvaluateAsync("Closing.ProfitLoss.UploadValidationV2", ruleContext);

        // Assert
        isValid.Should().BeFalse();
        validationErrors.Should().HaveCount(1);

        var error = validationErrors.First();
        error.Code.Should().Be("CLOSING_000");
        error.Message.Should().Be("No existen algunos de los conceptos solicitados");
    }

    [Fact]
    public async Task InternalRuleEvaluator_Should_Validate_At_Least_One_Income_Rule()
    {
        // Arrange
        var evaluator = _serviceProvider.GetRequiredService<IInternalRuleEvaluator<ClosingModuleMarker>>();

        var conceptNames = new[] { "Gastos Administrativos", "Otros Gastos" };
        var ruleContext = new
        {
            EffectiveDate = DateTime.UtcNow.Date,
            PortfolioCurrentDate = DateTime.UtcNow.Date.AddDays(-2),
            RequestedConceptNames = conceptNames,
            Concepts = new[]
            {
                new
                {
                    ProfitLossConceptId = 1L,
                    Concept = "Gastos Administrativos",
                    Nature = ProfitLossNature.Expense,
                    AllowNegative = false,
                    Amount = 100m,
                    IsIncome = false,
                    IsExpense = true
                },
                new
                {
                    ProfitLossConceptId = 2L,
                    Concept = "Otros Gastos",
                    Nature = ProfitLossNature.Expense,
                    AllowNegative = false,
                    Amount = 50m,
                    IsIncome = false,
                    IsExpense = true
                }
            }
        };

        // Act
        var (isValid, _, validationErrors) = await evaluator
            .EvaluateAsync("Closing.ProfitLoss.UploadValidationV2", ruleContext);

        // Assert
        isValid.Should().BeFalse();
        validationErrors.Should().HaveCount(1);

        var error = validationErrors.First();
        error.Code.Should().Be("CLOSING_005");
        error.Message.Should().Be("La fecha efectiva debe ser exactamente un día después de la fecha actual del portafolio");
    }

    [Fact]
    public async Task InternalRuleEvaluator_Should_Pass_When_All_Rules_Are_Valid()
    {
        // Arrange
        var evaluator = _serviceProvider.GetRequiredService<IInternalRuleEvaluator<ClosingModuleMarker>>();

        var conceptNames = new[] { "Rendimientos Brutos", "Gastos" };
        var portfolioCurrentDate = DateTime.UtcNow.Date.AddDays(-1);
        var effectiveDate = portfolioCurrentDate.AddDays(1);
        var ruleContext = new
        {
            EffectiveDate = effectiveDate,
            PortfolioCurrentDate = portfolioCurrentDate,
            RequestedConceptNames = conceptNames,
            Concepts = new[]
            {
                new
                {
                    ProfitLossConceptId = 1L,
                    Concept = "Rendimientos Brutos",
                    Nature = ProfitLossNature.Income,
                    AllowNegative = true,
                    Amount = 100m,
                    IsIncome = true,
                    IsExpense = false
                },
                new
                {
                    ProfitLossConceptId = 2L,
                    Concept = "Gastos",
                    Nature = ProfitLossNature.Expense,
                    AllowNegative = false,
                    Amount = 50m,
                    IsIncome = false,
                    IsExpense = true
                }
            }
        };

        // Act
        var (isValid, _, validationErrors) = await evaluator
            .EvaluateAsync("Closing.ProfitLoss.UploadValidationV2", ruleContext);

        // Assert
        isValid.Should().BeTrue();
        validationErrors.Should().BeEmpty();
    }

    private void Dispose()
    {
        _serviceProvider.GetService<IDisposable>()?.Dispose();
    }
}