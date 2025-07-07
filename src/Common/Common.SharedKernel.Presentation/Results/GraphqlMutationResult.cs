using Common.SharedKernel.Domain;

namespace Common.SharedKernel.Presentation.Results;

public class GraphqlMutationResult<T>
{
    public bool Success => Errors.Count == 0;
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<Error> Errors { get; set; } = [];

    public GraphqlMutationResult() { }

    public void AddError(Error error)
    {
        Errors.Add(error);
    }

    public void SetSuccess(T data, string? message = null)
    {
        Data = data;
        Message = message;
        Errors.Clear();
    }
}