
using Common.SharedKernel.Application.Rules;
using RulesEngine.Models;

namespace Common.SharedKernel.Application.Helpers.Rules;

public sealed class ExternalRuleEvaluatorAdapter<TModule> : IUnifiedRuleEvaluator<TModule>
{
    private readonly IRuleEvaluator<TModule> _inner;
    public ExternalRuleEvaluatorAdapter(IRuleEvaluator<TModule> inner) => _inner = inner ?? throw new ArgumentNullException(nameof(inner));

    public Task<(bool Success, IReadOnlyCollection<RuleResultTree> Results, IReadOnlyCollection<RuleValidationError> Errors)>
        EvaluateAsync<TInput>(string workflow, TInput input, CancellationToken ct = default)
        => _inner.EvaluateAsync(workflow, input, ct);

}