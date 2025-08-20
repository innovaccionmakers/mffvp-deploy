using Common.SharedKernel.Domain;
using FluentAssertions;
using Moq;
using Products.Application.Abstractions;
using Common.SharedKernel.Application.Rules;
using RulesEngine.Models;
using Products.Application.Objectives.GetObjectives;
using Products.Application.Objectives.Services;
using Common.SharedKernel.Core.Primitives;

namespace Products.test.UnitTests.Application.Services;

public class GetObjectivesRulesTests
{
    private readonly Mock<IRuleEvaluator<ProductsModuleMarker>> _evaluator = new();

    [Fact]
    public async Task EvaluateAsync_Should_Return_Success_When_Evaluator_Succeeds()
    {
        _evaluator.Setup(e => e.EvaluateAsync("Products.Objective.ValidateGetObjectives",
                It.IsAny<ObjectiveValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));
        var service = new GetObjectivesRules(_evaluator.Object);

        var result = await service.EvaluateAsync(new ObjectiveValidationContext(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_Should_Return_Failure_When_Evaluator_Fails()
    {
        var error = new RuleValidationError("R1", "fail");
        _evaluator.Setup(e => e.EvaluateAsync("Products.Objective.ValidateGetObjectives",
                It.IsAny<ObjectiveValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { error }));
        var service = new GetObjectivesRules(_evaluator.Object);

        var result = await service.EvaluateAsync(new ObjectiveValidationContext(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        AssertionExtensions.Should(result.Error).BeEquivalentTo(Error.Validation("R1", "fail"));
    }
}