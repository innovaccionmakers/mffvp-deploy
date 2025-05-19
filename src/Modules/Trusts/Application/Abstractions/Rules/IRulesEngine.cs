using RulesEngine.Models;

namespace Trusts.Application.Abstractions.Rules;

public interface IRulesEngine<TModule>
{
    ValueTask<List<RuleResultTree>> ExecuteAllRulesAsync(
        string workflow,
        params RuleParameter[] inputs);
}