

using Common.SharedKernel.Application.Rules;
using RulesEngine.Models;

namespace Common.SharedKernel.Application.Helpers.Rules;

public interface IUnifiedRuleEvaluator<TModule>
{
    Task<(bool Success, IReadOnlyCollection<RuleResultTree> Results, IReadOnlyCollection<RuleValidationError> Errors)>
        EvaluateAsync<TInput>(string workflow, TInput input, CancellationToken ct = default);
}
