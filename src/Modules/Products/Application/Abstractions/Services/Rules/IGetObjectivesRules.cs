using Common.SharedKernel.Domain;
using Products.Application.Objectives.GetObjectives;

namespace Products.Application.Abstractions.Services.Rules;

public interface IGetObjectivesRules
{
    Task<Result> EvaluateAsync(ObjectiveValidationContext context, CancellationToken ct);
}