using Contributions.Application.Abstractions.Rules;
using Microsoft.Extensions.Logging;
using RulesEngine.Interfaces;
using RulesEngine.Models;

namespace Contributions.Infrastructure.RulesEngine
{
    internal sealed class RuleEvaluator : IRuleEvaluator
    {
        private readonly IRulesEngine _engine;
        private readonly ILogger<RuleEvaluator> _log;

        public RuleEvaluator(IRulesEngine engine, ILogger<RuleEvaluator> log)
        {
            _engine = engine;
            _log = log;
        }

        public async Task<(bool Success, IReadOnlyCollection<RuleResultTree> Results,
                           IReadOnlyCollection<RuleValidationError> Errors)>
            EvaluateAsync<T>(string workflow, T input, CancellationToken ct = default)
        {
            using var scope = _log.BeginScope("Workflow:{Workflow}", workflow);

            var ruleParams = new[] { new RuleParameter("input", input) };
            var results = await _engine.ExecuteAllRulesAsync(workflow, ruleParams);

            if (results.Count == 0)
            {
                _log.LogWarning("No rules were executed for workflow {Workflow}", workflow);
            }

            var errors = results.Where(r => !r.IsSuccess)
                                .Select(r => new RuleValidationError(
                                    r.Rule.RuleName,
                                    r.ExceptionMessage ?? r.Rule.ErrorMessage ?? "Validation error"))
                                .ToArray();

            var success = errors.Length == 0;

            _log.LogDebug("RuleEngine finished – success: {Success}", success);
            return (success, results, errors);
        }
    }
}
