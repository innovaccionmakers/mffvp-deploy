using Microsoft.Extensions.Logging;
using RulesEngine.Interfaces;
using RulesEngine.Models;
using Trusts.Application.Abstractions;
using Trusts.Application.Abstractions.Rules;

namespace Trusts.Infrastructure.RulesEngine;

internal sealed class RuleEvaluator<TModule> : IRuleEvaluator<TModule>
{
    private readonly IErrorCatalog _catalog;
    private readonly IRulesEngine<TrustsModuleMarker> _engine;
    private readonly ILogger<TModule> _log;

    public RuleEvaluator(IRulesEngine<TrustsModuleMarker> engine,
        IErrorCatalog catalog,
        ILogger<TModule> log)
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

        var firstFail = results.FirstOrDefault(r => !r.IsSuccess);
        if (firstFail is null)
            return (true, results, Array.Empty<RuleValidationError>());

        if (!(firstFail.Rule.Properties?.TryGetValue("errorCode", out var codeObj) ?? false))
        {
            _log.LogError(
                "La regla '{RuleName}' no contiene la propiedad 'errorCode'.",
                firstFail.Rule.RuleName
            );
            return (
                false,
                results,
                new[]
                {
                    new RuleValidationError(
                        "UNKNOWN_ERROR",
                        firstFail.Rule.ErrorMessage
                        ?? $"Regla inválida ({firstFail.Rule.RuleName})"
                    )
                }
            );
        }

        var codeStr = codeObj?.ToString() ?? string.Empty;
        if (!Guid.TryParse(codeStr, out var ruleUuid))
        {
            _log.LogError(
                "El errorCode '{ErrorCode}' no es un GUID válido para la regla '{RuleName}'.",
                codeStr,
                firstFail.Rule.RuleName
            );
            return (
                false,
                results,
                new[]
                {
                    new RuleValidationError(
                        "INVALID_ERROR_CODE",
                        firstFail.Rule.ErrorMessage
                        ?? $"Error en regla ({firstFail.Rule.RuleName})"
                    )
                }
            );
        }

        var (code, message) = await _catalog.GetAsync(ruleUuid, ct);

        return (
            false,
            results,
            new[]
            {
                new RuleValidationError(code, message)
            }
        );
    }
}