using RulesEngine.Models;

namespace Operations.Application.Abstractions.Rules;

public record RuleValidationError(string Code, string Message);

public interface IRuleEvaluator<TModule>
{
    Task<(bool Success,
            IReadOnlyCollection<RuleResultTree> Results,
            IReadOnlyCollection<RuleValidationError> Errors)>
        EvaluateAsync<T>(string workflow, T input,
            CancellationToken ct = default);
}