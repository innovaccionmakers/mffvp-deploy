using RulesEngine.Models;

namespace Common.SharedKernel.Application.Rules;

public interface IRuleEvaluator<TModule>
{
    Task<(bool Success,
            IReadOnlyCollection<RuleResultTree> Results,
            IReadOnlyCollection<RuleValidationError> Errors)>
        EvaluateAsync<T>(string workflow, T input,
            CancellationToken ct = default);
}
