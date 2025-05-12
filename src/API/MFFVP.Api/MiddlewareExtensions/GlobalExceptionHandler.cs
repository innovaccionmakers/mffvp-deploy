using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MFFVP.Api.MiddlewareExtensions;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext ctx,
        Exception ex,
        CancellationToken cancellationToken)
    {
        var traceId = ctx.Items["TraceId"]?.ToString();

        logger.LogError(ex, "Unhandled exception. TraceId={TraceId}", traceId);

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server failure",
            Type = "https://www.rfc-editor.org/rfc/rfc7231#section-6.6.1",
            Detail = ex.Message
        };

        if (traceId is not null)
            problem.Extensions["traceId"] = traceId;

        ctx.Response.StatusCode = problem.Status!.Value;
        await ctx.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}