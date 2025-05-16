using Associate.Application.Abstractions.Rules;
using Microsoft.Extensions.Logging;
using RulesEngine.Interfaces;
using RulesEngine.Models;

namespace Associate.Infrastructure.RulesEngine;

internal sealed class RuleEvaluator : IRuleEvaluator
{
    private readonly IErrorCatalog _catalog;
    private readonly IRulesEngine _engine;
    private readonly ILogger<RuleEvaluator> _log;

    public RuleEvaluator(IRulesEngine engine,
        IErrorCatalog catalog,
        ILogger<RuleEvaluator> log)
    {
        _engine = engine;
        _catalog = catalog;
        _log = log;
    }

    public async Task<(bool Success,
            IReadOnlyCollection<RuleResultTree> Results,
            IReadOnlyCollection<RuleValidationError> Errors)>
        EvaluateAsync<T>(string workflow, T input, CancellationToken ct = default)
    {
        var ruleParams = new[] { new RuleParameter("input", input) };
        var results = await _engine.ExecuteAllRulesAsync(workflow, ruleParams);

        var errors = new List<RuleValidationError>();

        foreach (var r in results.Where(r => !r.IsSuccess))
        {
            var (num, defaultMsg) = await _catalog.GetAsync(r.Rule.RuleName, ct);

            var message = r.ExceptionMessage
                          ?? r.Rule.ErrorMessage
                          ?? defaultMsg;

            errors.Add(new RuleValidationError(num.ToString(), message));
        }

        var success = errors.Count == 0;
        return (success, results, errors);
    }
}