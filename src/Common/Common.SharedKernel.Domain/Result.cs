using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace Common.SharedKernel.Domain;

public class Result
{
    public Result(bool isSuccess, Error error, string? description = null)
    {
        if ((isSuccess && error != Error.None) ||
            (!isSuccess && error == Error.None))
            throw new ArgumentException("Invalid error", nameof(error));

        IsSuccess = isSuccess;
        Error = error;        
        Description = description ?? string.Empty;
    }

    public bool IsSuccess { get; }
    public string Description { get;  }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success()
    {
        return new Result(true, Error.None);
    }

    public static Result Success(string description)
    {
        return new Result(true, Error.None, description);
    }

    public static Result<TValue> Success<TValue>(TValue value, string? description = null)
    {
        return new Result<TValue>(value, true, Error.None, description);
    }

    public static Result Failure(Error error)
    {
        return new Result(false, error);
    }

    public static Result<TValue> Failure<TValue>(Error error)
    {
        return new Result<TValue>(default, false, error);
    }

    public object Match(Func<object?, IResult> ok, Func<Result, IResult> problem)
    {
        throw new NotImplementedException();
    }
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public Result(TValue? value, bool isSuccess, Error error, string? description = null)
        : base(isSuccess, error, description)
    {
        _value = value;
    }

    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    public static implicit operator Result<TValue>(TValue? value)
    {
        return value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
    }

    public static Result<TValue> ValidationFailure(Error error)
    {
        return new Result<TValue>(default, false, error);
    }
}