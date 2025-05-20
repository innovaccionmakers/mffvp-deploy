using RulesEngine.Models;

namespace Operations.Application.Abstractions.Rules;

public interface IRulesEngine<TModule>
{
    ValueTask<List<RuleResultTree>> ExecuteAllRulesAsync(
        string workflow,
        params RuleParameter[] inputs);
}