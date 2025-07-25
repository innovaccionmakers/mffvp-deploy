﻿namespace MFFVP.Api.MiddlewareExtensions;

internal static class MiddlewareExtensions
{
    internal static IApplicationBuilder UseLogContext(this IApplicationBuilder app)
    {
        app.UseMiddleware<LogContextTraceLoggingMiddleware>();

        return app;
    }
}