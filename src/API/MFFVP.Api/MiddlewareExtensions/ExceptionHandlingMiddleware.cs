using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace MFFVP.Api.MiddlewareExtensions
{
    public class ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (JsonException jsonEx)
            {
                await HandleJsonException(context, jsonEx);
            }
            catch (BadHttpRequestException badReqEx) when (badReqEx.InnerException is JsonException)
            {
                await HandleJsonException(context, badReqEx.InnerException as JsonException);
            }
            catch (Exception ex)
            {
                await HandleServerError(context, ex);
            }
        }

        private async Task HandleJsonException(HttpContext context, JsonException jsonEx)
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

            await WriteProblemDetails(context, problem, traceId, new
            {
                path = jsonEx.Path,
                lineNumber = jsonEx.LineNumber,
                bytePositionInLine = jsonEx.BytePositionInLine
            });
        }

        private async Task HandleServerError(HttpContext context, Exception ex)
        {
            var traceId = context.Items["TraceId"]?.ToString();
            logger.LogError(ex, "Unhandled exception. TraceId={TraceId}", traceId);

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                Detail = ex.Message
            };
            await WriteProblemDetails(context, problem, traceId);
        }

        private async Task WriteProblemDetails(HttpContext context, ProblemDetails problem, string traceId, object errors = null)
        {
            if (traceId is not null)
                problem.Extensions["traceId"] = traceId;

            if (errors is not null)
                problem.Extensions["errors"] = errors;

            context.Response.StatusCode = problem.Status.Value;
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}