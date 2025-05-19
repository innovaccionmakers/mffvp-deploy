using People.Application.Abstractions.Rules;
using RulesEngine.Models;

namespace People.Infrastructure.RulesEngine;

public static class RulesEngineExtensions
{
    public static IRulesEngine<T> AsGeneric<T>(
        this global::RulesEngine.RulesEngine re)
    {
        return new RulesEngineWrapper<T>(re);
    }

    private sealed class RulesEngineWrapper<T> : IRulesEngine<T>
    {
        private readonly global::RulesEngine.RulesEngine _inner;

        public RulesEngineWrapper(global::RulesEngine.RulesEngine inner)
        {
            _inner = inner;
        }

        public ValueTask<List<RuleResultTree>> ExecuteAllRulesAsync(
            string workflow,
            params RuleParameter[] inputs)
        {
            return _inner.ExecuteAllRulesAsync(workflow, inputs);
        }
    }
}