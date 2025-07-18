using Common.SharedKernel.Application.Rules;
using RulesEngine.Models;

namespace Common.SharedKernel.Application.Helpers.Rules;

public sealed class WorkflowValidationResult
{
    private WorkflowValidationResult(bool isValid, IReadOnlyList<RuleValidationError> errors, IReadOnlyDictionary<string, IReadOnlyCollection<RuleResultTree>> resultsByWorkflow)
    {
        IsValid = isValid;
        Errors = errors;
        ResultsByWorkflow = resultsByWorkflow;
    }

    public bool IsValid { get; }
    public IReadOnlyList<RuleValidationError> Errors { get; }
    public IReadOnlyDictionary<string, IReadOnlyCollection<RuleResultTree>> ResultsByWorkflow { get; }

    public static WorkflowValidationResult Success(IDictionary<string, IReadOnlyCollection<RuleResultTree>>? results = null) =>
        new(true, Array.Empty<RuleValidationError>(), results?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, IReadOnlyCollection<RuleResultTree>>());

    public static WorkflowValidationResult Failure(IEnumerable<RuleValidationError> errors, IDictionary<string, IReadOnlyCollection<RuleResultTree>>? results = null) =>
        new(false, errors.ToList(), results?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, IReadOnlyCollection<RuleResultTree>>());
}