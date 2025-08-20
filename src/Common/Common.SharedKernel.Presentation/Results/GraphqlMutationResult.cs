using Common.SharedKernel.Core.Primitives;

namespace Common.SharedKernel.Presentation.Results;


public class GraphqlMutationResult
{
    public bool Success => Errors.Count == 0;
    public string? Message { get; set; }
    public List<Error> Errors { get; set; } = [];

    public GraphqlMutationResult() { }

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

public class GraphqlMutationResult<T> : GraphqlMutationResult
{
    public T? Data { get; set; }

    public GraphqlMutationResult() { }

    public void SetSuccess(T data, string? message = null)
    {
        Data = data;
        Message = message;
        Errors.Clear();
    }
}