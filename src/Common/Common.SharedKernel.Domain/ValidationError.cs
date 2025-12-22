using Common.SharedKernel.Core.Primitives;

namespace Common.SharedKernel.Domain;

public sealed record ValidationError : Error
{
    public ValidationError(Error[] errors)
        : base(
            "General.Validation",
            // Build a meaningful description from individual errors instead of a generic message
            errors is null || errors.Length == 0
                ? "One or more validation errors occurred"
                : string.Join("; ", errors.Select(e => e.Description)),
            ErrorType.Validation)
    {
        Errors = errors ?? System.Array.Empty<Error>();
    }

    public Error[] Errors { get; }

    public static ValidationError FromResults(IEnumerable<Result> results)
    {
        return new ValidationError(results.Where(r => r.IsFailure).Select(r => r.Error).ToArray());
    }
}