using Microsoft.Extensions.Logging;
using Common.SharedKernel.Application.Rules;
using RulesEngine.Interfaces;
using RulesEngine.Models;

namespace Common.SharedKernel.Infrastructure.RulesEngine;

internal sealed class InternalRuleEvaluator<TModule> : IInternalRuleEvaluator<TModule>
{
    private readonly IRulesEngine<TModule> _engine;
    private readonly ILogger<TModule> _log;

    public InternalRuleEvaluator(
        IRulesEngine<TModule> engine,
        ILogger<TModule> log
    )
    {
        _engine = engine;
        _log = log;
    }

    public async Task<(
        bool Success,
        IReadOnlyCollection<RuleResultTree> Results,
        IReadOnlyCollection<RuleValidationError> Errors
        )> EvaluateAsync<T>(
        string workflow,
        T input,
        CancellationToken ct = default
    )
    {
        var ruleParams = new[] { new RuleParameter("input", input) };
        var results = await _engine.ExecuteAllRulesAsync(workflow, ruleParams);

        var firstFail = results.FirstOrDefault(r => !r.IsSuccess);
        if (firstFail is null) return (true, results, Array.Empty<RuleValidationError>());
        
        string errorCode;
        if (firstFail.Rule.Properties?.TryGetValue("internalCode", out var internalCodeObj) == true)
        {
            errorCode = internalCodeObj?.ToString() ?? "UNKNOWN_INTERNAL_ERROR";
        }
        else if (firstFail.Rule.Properties?.TryGetValue("errorCode", out var uuidCodeObj) == true)
        {
            errorCode = uuidCodeObj?.ToString() ?? "UNKNOWN_ERROR";
        }
        else
        {
            _log.LogError(
                "La regla '{RuleName}' no contiene ni 'internalCode' ni 'errorCode'.",
                firstFail.Rule.RuleName
            );
            errorCode = "UNKNOWN_ERROR";
        }
        
        var errorMessage = firstFail.Rule.ErrorMessage ?? $"Regla inválida ({firstFail.Rule.RuleName})";

        return (
            false,
            results,
            new[]
            {
                new RuleValidationError(errorCode, errorMessage)
            }
        );
    }
}