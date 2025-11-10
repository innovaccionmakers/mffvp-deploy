namespace Common.SharedKernel.Domain;

public class ProcessingResult<T, E>
{
    public IEnumerable<T> SuccessItems { get; }
    public IEnumerable<E> Errors { get; }

    public bool IsSuccess => !Errors.Any();

    public ProcessingResult(IEnumerable<T> successItems, IEnumerable<E> errors)
    {
        SuccessItems = successItems ?? [];
        Errors = errors ?? [];
    }
}
