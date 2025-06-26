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

namespace Closing.test.UnitTests.Application.ProfitLosses;

public class InternalRuleEvaluatorIntegrationTests
{
    private readonly IServiceProvider _serviceProvider;

    public InternalRuleEvaluatorIntegrationTests()
    {
        var services = new ServiceCollection();

        services.AddRulesEngine<ClosingModuleMarker>(typeof(ClosingModuleMarker).Assembly);

        services.AddLogging(builder => builder.AddConsole());

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task InternalRuleEvaluator_Should_Return_Internal_Code_From_Rules_Json()
    {
        // Arrange
        var evaluator = _serviceProvider.GetRequiredService<IInternalRuleEvaluator<ClosingModuleMarker>>();

        var ruleContext = new
        {
            EffectiveDate = DateTime.UtcNow.AddDays(1),
            Concepts = new[]
            {
                new
                {
                    ProfitLossConceptId = 1L,
                    Concept = "Rendimientos Brutos",
                    Nature = ProfitLossNature.Income,
                    AllowNegative = true,
                    Amount = 100m
                },
                new
                {
                    ProfitLossConceptId = 2L,
                    Concept = "Gastos",
                    Nature = ProfitLossNature.Expense,
                    AllowNegative = false,
                    Amount = 50m
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

        var ruleContext = new
        {
            EffectiveDate = DateTime.UtcNow.Date,
            Concepts = new[]
            {
                new
                {
                    ProfitLossConceptId = 1L,
                    Concept = "Rendimientos Brutos",
                    Nature = ProfitLossNature.Income,
                    AllowNegative = true,
                    Amount = 100m
                },
                new
                {
                    ProfitLossConceptId = 2L,
                    Concept = "Gastos",
                    Nature = ProfitLossNature.Expense,
                    AllowNegative = false,
                    Amount = -50m
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
    public async Task InternalRuleEvaluator_Should_Validate_At_Least_One_Income_Rule()
    {
        // Arrange
        var evaluator = _serviceProvider.GetRequiredService<IInternalRuleEvaluator<ClosingModuleMarker>>();

        var ruleContext = new
        {
            EffectiveDate = DateTime.UtcNow.Date,
            Concepts = new[]
            {
                new
                {
                    ProfitLossConceptId = 1L,
                    Concept = "Gastos Administrativos",
                    Nature = ProfitLossNature.Expense, // Solo gastos, sin ingresos
                    AllowNegative = false,
                    Amount = 100m
                },
                new
                {
                    ProfitLossConceptId = 2L,
                    Concept = "Otros Gastos",
                    Nature = ProfitLossNature.Expense,
                    AllowNegative = false,
                    Amount = 50m
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
        error.Code.Should().Be("CLOSING_003");
        error.Message.Should().Be("Debe existir al menos un concepto de naturaleza Ingreso");
    }

    [Fact]
    public async Task InternalRuleEvaluator_Should_Pass_When_All_Rules_Are_Valid()
    {
        // Arrange
        var evaluator = _serviceProvider.GetRequiredService<IInternalRuleEvaluator<ClosingModuleMarker>>();

        var ruleContext = new
        {
            EffectiveDate = DateTime.UtcNow.Date,
            Concepts = new[]
            {
                new
                {
                    ProfitLossConceptId = 1L,
                    Concept = "Rendimientos Brutos",
                    Nature = ProfitLossNature.Income,
                    AllowNegative = true,
                    Amount = 100m
                },
                new
                {
                    ProfitLossConceptId = 2L,
                    Concept = "Gastos",
                    Nature = ProfitLossNature.Expense,
                    AllowNegative = false,
                    Amount = 50m
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