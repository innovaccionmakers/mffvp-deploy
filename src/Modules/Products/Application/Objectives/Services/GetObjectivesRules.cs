using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Products.Application.Abstractions;
using Products.Application.Abstractions.Services.Rules;
using Products.Application.Objectives.GetObjectives;

namespace Products.Application.Objectives.Services;

public sealed class GetObjectivesRules(
    IRuleEvaluator<ProductsModuleMarker> evaluator) : IGetObjectivesRules
{
    private const string Flow = "Products.Objective.ValidateGetObjectives";

    public async Task<Result> EvaluateAsync(
        ObjectiveValidationContext ctx, CancellationToken ct)
    {
        var (ok, _, errors) = await evaluator.EvaluateAsync(Flow, ctx, ct);

        if (ok) return Result.Success();

        var first = errors.First();
        return Result.Failure(Error.Validation(first.Code, first.Message));
    }
}