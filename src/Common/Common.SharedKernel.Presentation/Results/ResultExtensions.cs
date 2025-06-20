using Common.SharedKernel.Domain;
using Microsoft.AspNetCore.Http;

namespace Common.SharedKernel.Presentation.Results;

public static class ResultExtensions
{
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<Result, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess() : onFailure(result);
    }

    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Result<TIn>, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result);
    }

    public static IResult ToApiResult<T>(this Result<T> result, string? description = null)
    {
        var desc = string.IsNullOrWhiteSpace(description) ? result.Description : description;
        return result.IsSuccess
            ? ApiResults.Ok(result.Value, desc ?? string.Empty)
            : ApiResults.Failure(result);
    }

    public static IResult ToApiResult(this Result result, string? description = null)
    {
        var desc = string.IsNullOrWhiteSpace(description) ? result.Description : description;
        return result.IsSuccess
            ? ApiResults.Ok(result.IsSuccess, desc ?? string.Empty)
            : ApiResults.Failure(result);
    }
}