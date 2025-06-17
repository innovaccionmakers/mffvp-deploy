using System.Diagnostics;
using Serilog.Context;

namespace MFFVP.Api.MiddlewareExtensions;

internal sealed class LogContextTraceLoggingMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        context.Items["TraceId"] = traceId;

        context.Response.OnStarting(() =>
        {
            if (context.Response.StatusCode >= 500)
                context.Response.Headers.TryAdd("X-Trace-Id", traceId);
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("TraceId", traceId))
        {
            await next(context);
        }
    }
}