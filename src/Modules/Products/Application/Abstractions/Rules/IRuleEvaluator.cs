using RulesEngine.Models;

namespace Products.Application.Abstractions.Rules;

public record RuleValidationError(string Code, string Message);

public interface IRuleEvaluator
{
    Task<(bool Success, IReadOnlyCollection<RuleResultTree> Results,
            IReadOnlyCollection<RuleValidationError> Errors)>
        EvaluateAsync<T>(string workflow, T input, CancellationToken ct = default);
}