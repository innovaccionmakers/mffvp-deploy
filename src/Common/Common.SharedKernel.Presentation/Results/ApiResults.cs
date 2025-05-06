using Common.SharedKernel.Domain;
using Microsoft.AspNetCore.Http;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Common.SharedKernel.Presentation.Results;

public static class ApiResults
{
    public static IResult Ok<T>(T payload) =>
        ApiSuccessBuilder.Build(payload);

    public static IResult Failure(Result failure)
    {
        int code = int.TryParse(failure.Error.Code, out var parsed) ? parsed : 6000;

        return HttpResults.Json(
            new
            {
                Estado = "Fallida",
                CodigoRespuesta = code,
                DescripcionRespuesta = failure.Error.Description
            },
            statusCode: StatusCodes.Status400BadRequest);
    }

    public static IResult Problem(Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot create Problem from a success.");

        return HttpResults.Problem(
            title: result.Error.Code,
            detail: result.Error.Description,
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            statusCode: StatusCodes.Status400BadRequest);
    }
}
