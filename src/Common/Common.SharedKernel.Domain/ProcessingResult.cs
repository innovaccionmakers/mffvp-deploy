namespace Common.SharedKernel.Domain;
using Common.SharedKernel.Core.Primitives;

public class ProcessingResult<T>
{
    public IEnumerable<T> SuccessItems { get; }
    public IEnumerable<Error> Errors { get; }

    public bool IsSuccess => !Errors.Any();

    public ProcessingResult(IEnumerable<T> successItems, IEnumerable<Error> errors)
    {
        SuccessItems = successItems ?? [];
        Errors = errors ?? [];
    }
}
