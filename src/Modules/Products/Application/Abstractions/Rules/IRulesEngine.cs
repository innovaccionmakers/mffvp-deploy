using RulesEngine.Models;

namespace Products.Application.Abstractions.Rules;

public interface IRulesEngine<TModule>
{
    ValueTask<List<RuleResultTree>> ExecuteAllRulesAsync(
        string workflow,
        params RuleParameter[] inputs);
}