using RulesEngine.Models;

namespace Common.SharedKernel.Application.Rules;

public interface IRulesEngine<TModule>
{
    ValueTask<List<RuleResultTree>> ExecuteAllRulesAsync(
        string workflow,
        params RuleParameter[] inputs);
}
