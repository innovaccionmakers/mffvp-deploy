using Common.SharedKernel.Core.Primitives;

namespace Common.SharedKernel.Presentation.Results;


public class GraphqlResult
{
    public bool Success => Errors.Count == 0;
    public string? Message { get; set; }
    public List<Error> Errors { get; set; } = [];

    public GraphqlResult() { }

    public void AddError(Error error)
    {
        Errors.Add(error);
    }

    public void SetSuccess(string? message = null)
    {
        Message = message;
        Errors.Clear();
    }
}

public class GraphqlResult<T> : GraphqlResult
{
    public T? Data { get; set; }

    public GraphqlResult() { }

    public void SetSuccess(T data, string? message = null)
    {
        Data = data;
        Message = message;
        Errors.Clear();
    }
}