using System.Diagnostics;
using Common.SharedKernel.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Common.SharedKernel.Presentation.Results;

public static class ApiResults
{
    public static IResult Ok<T>(T payload)
    {
        return ApiSuccessBuilder.Build(payload);
    }

    public static IResult Failure(Result failure)
    {
        var code = int.TryParse(failure.Error.Code, out var parsed) ? parsed : 6000;

        return HttpResults.Json(
            new
            {
                Estado = "Fallida",
                CodigoRespuesta = code,
                DescripcionRespuesta = failure.Error.Description,
                traceId = GetTraceId()
            },
            statusCode: StatusCodes.Status400BadRequest);
    }

    public static IResult Problem(Result result)
    {
        return BuildProblem(result.Error);
    }

    public static IResult Problem<T>(Result<T> result)
    {
        return BuildProblem(result.Error);
    }

    private static IResult BuildProblem(Error error)
    {
        var traceId = GetTraceId();

        var pd = new ProblemDetails
        {
            Title = error.Code,
            Detail = error.Description,
            Status = error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            },
            Type = error.Type switch
            {
                ErrorType.Validation => "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.1",
                ErrorType.NotFound => "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.4",
                ErrorType.Conflict => "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.8",
                _ => "about:blank"
            }
        };

        if (!string.IsNullOrWhiteSpace(traceId))
            pd.Extensions["traceId"] = traceId;

        if (error is ValidationError ve)
            pd.Extensions["errors"] = ve.Errors.Select(e => new { e.Code, e.Description });

        return HttpResults.Problem(pd);
    }

    private static string? GetTraceId()
    {
        return Activity.Current?.TraceId.ToString();
    }
}