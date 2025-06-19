using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace MFFVP.Api.MiddlewareExtensions
{
    public class JsonExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<JsonExceptionHandlingMiddleware> logger)
    {
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (JsonException jsonEx)
            {
                var traceId = context.Items["TraceId"]?.ToString();
                logger.LogError(jsonEx, "JSON parsing error. TraceId={TraceId}", traceId);

                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid JSON format",
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                    Detail = jsonEx.Message
                };

                if (traceId is not null)
                    problem.Extensions["traceId"] = traceId;

                problem.Extensions["errors"] = new
                {
                    path = jsonEx.Path,
                    lineNumber = jsonEx.LineNumber,
                    bytePositionInLine = jsonEx.BytePositionInLine
                };

                context.Response.StatusCode = problem.Status.Value;
                await context.Response.WriteAsJsonAsync(problem);
            }
            catch (BadHttpRequestException badReqEx) when (badReqEx.InnerException is JsonException)
            {
                var traceId = context.Items["TraceId"]?.ToString();
                var jsonEx = badReqEx.InnerException as JsonException;
                logger.LogError(jsonEx, "Invalid request data. TraceId={TraceId}", traceId);

                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid request data",
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                    Detail = jsonEx.Message
                };

                if (traceId is not null)
                    problem.Extensions["traceId"] = traceId;

                problem.Extensions["errors"] = new
                {
                    path = jsonEx.Path,
                    lineNumber = jsonEx.LineNumber,
                    bytePositionInLine = jsonEx.BytePositionInLine
                };

                context.Response.StatusCode = problem.Status.Value;
                await context.Response.WriteAsJsonAsync(problem);
            }
        }
    }
}