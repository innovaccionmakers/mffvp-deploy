
using Common.SharedKernel.Application.Rules;
using RulesEngine.Models;

namespace Common.SharedKernel.Application.Helpers.Rules;

public sealed class GenericWorkflowValidator<TModule>
{
    private readonly IUnifiedRuleEvaluator<TModule> _ruleEvaluator;

    public GenericWorkflowValidator(IUnifiedRuleEvaluator<TModule> ruleEvaluator)
    {
        _ruleEvaluator = ruleEvaluator ?? throw new ArgumentNullException(nameof(ruleEvaluator));
    }

    /// <summary>
    /// Evalua un solo workflow.
    /// </summary>
    public async Task<WorkflowValidationResult> EvaluateAsync<TContext>(
        string workflowName,
        TContext context,
        ErrorSelection errorSelection = ErrorSelection.First,
        CancellationToken ct = default)
    {
        var (success, results, errors) = await _ruleEvaluator.EvaluateAsync(workflowName, context, ct);

        if (success)
        {
            return WorkflowValidationResult.Success(new Dictionary<string, IReadOnlyCollection<RuleResultTree>>
                {
                    { workflowName, results }
                });
        }

        var surfaced = SurfaceErrors(errors, errorSelection);
        return WorkflowValidationResult.Failure(surfaced, new Dictionary<string, IReadOnlyCollection<RuleResultTree>>
            {
                { workflowName, results }
            });
    }

    /// <summary>
    /// Evalua muchos workflows en el orden especificado.
    /// </summary>
    /// <param name="workflowNames">Lista en orden de evaluación.</param>
    /// <param name="context">El contexto de entrada pasado a cada workflow.</param>
    /// <param name="errorSelection">Devuelve el primer error por fallo o agrega todos los errores encontrados.</param>
    /// <param name="mode">Circuito corto en el primer fallo o evalúa todos los workflows.</param>
    public async Task<WorkflowValidationResult> EvaluateManyAsync<TContext>(
        IReadOnlyList<string> workflowNames,
        TContext context,
        ErrorSelection errorSelection = ErrorSelection.First,
        WorkflowEvaluationMode mode = WorkflowEvaluationMode.ShortCircuitOnFailure,
        CancellationToken ct = default)
    {
        if (workflowNames == null || workflowNames.Count == 0)
            throw new ArgumentException("Se debe especificar al menos un workflow.", nameof(workflowNames));

        var allErrors = new List<RuleValidationError>();
        var resultsByWorkflow = new Dictionary<string, IReadOnlyCollection<RuleResultTree>>();

        foreach (var wf in workflowNames)
        {
            var (success, results, errors) = await _ruleEvaluator.EvaluateAsync(wf, context, ct);
            resultsByWorkflow[wf] = results;

            if (!success)
            {
                var surfaced = SurfaceErrors(errors, errorSelection);
                allErrors.AddRange(surfaced);

                if (mode == WorkflowEvaluationMode.ShortCircuitOnFailure)
                {
                    // detener; devolver solo lo que tenemos hasta ahora
                    return WorkflowValidationResult.Failure(allErrors, resultsByWorkflow);
                }
            }
        }

        if (allErrors.Count == 0)
            return WorkflowValidationResult.Success(resultsByWorkflow);

        return WorkflowValidationResult.Failure(allErrors, resultsByWorkflow);
    }

    private static IReadOnlyList<RuleValidationError> SurfaceErrors(IReadOnlyCollection<RuleValidationError> errors, ErrorSelection selection)
    {
        if (errors == null || errors.Count == 0)
            return Array.Empty<RuleValidationError>();

        if (selection == ErrorSelection.First)
            return new[] { errors.First() };

        return errors.ToList();
    }
}