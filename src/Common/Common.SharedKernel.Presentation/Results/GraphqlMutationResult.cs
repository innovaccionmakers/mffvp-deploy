namespace Common.SharedKernel.Presentation.Results;

public class GraphqlMutationResult<T>
{
    public bool Success => Errors.Count == 0;
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<ErrorMutationResult> Errors { get; set; } = [];

    public GraphqlMutationResult() { }

    public void AddError(string error, string errorDetail)
    {
        Errors.Add(new ErrorMutationResult { ErrorMessage = error, ErrorDetail = errorDetail });
    }

    public void SetSuccess(T data, string? message = null)
    {
        Data = data;
        Message = message;
        Errors.Clear();
    }
}

public class ErrorMutationResult
{
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorDetail { get; set; } = string.Empty;
}
