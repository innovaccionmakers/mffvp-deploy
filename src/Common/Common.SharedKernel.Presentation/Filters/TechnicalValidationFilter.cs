using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.SharedKernel.Presentation.Filters;

public sealed class TechnicalValidationFilter<TRequest> : IEndpointFilter
    where TRequest : class
{
    private readonly ILogger<TechnicalValidationFilter<TRequest>> _log;

    public TechnicalValidationFilter(ILogger<TechnicalValidationFilter<TRequest>> log)
    {
        _log = log;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext ctx,
        EndpointFilterDelegate next)
    {
        var validator = ctx.HttpContext.RequestServices.GetService<IValidator<TRequest>>();
        if (validator is null) return await next(ctx);

        var request = ctx.Arguments.OfType<TRequest>().FirstOrDefault();
        if (request is null) return await next(ctx);

        var validation = await validator.ValidateAsync(request);
        if (validation.IsValid) return await next(ctx);

        var traceId = ctx.HttpContext.Items["TraceId"]?.ToString();

        var errors = validation.Errors
            .Select(e => Error.Validation($"Validation.{e.PropertyName}", e.ErrorMessage))
            .ToArray();

        _log.LogWarning(
            "Validation failed for {RequestType}. TraceId={TraceId}. Errors={@Errors}",
            typeof(TRequest).Name,
            traceId,
            errors);

        var result = Result.Failure(new ValidationError(errors));
        return ApiResults.Problem(result);
    }
}