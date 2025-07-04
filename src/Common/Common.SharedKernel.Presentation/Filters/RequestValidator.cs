using Common.SharedKernel.Domain;

using FluentValidation;

using Microsoft.Extensions.Logging;

namespace Common.SharedKernel.Presentation.Filters;

public static class RequestValidator
{
    public static async Task<Result?> Validate<TRequest>(
        TRequest request,
        IValidator<TRequest> validator,
        ILogger? logger = null,
        string? traceId = null)
        where TRequest : class
    {
        var validation = await validator.ValidateAsync(request);
        if (validation.IsValid) return null;

        var errors = validation.Errors
            .Select(e => Error.Validation($"Validation.{e.PropertyName}", e.ErrorMessage))
            .ToArray();

        logger?.LogWarning(
            "Validation failed for {RequestType}. TraceId={TraceId}. Errors={@Errors}",
            typeof(TRequest).Name,
            traceId,
            errors);

        return Result.Failure(new ValidationError(errors));
    }
}
