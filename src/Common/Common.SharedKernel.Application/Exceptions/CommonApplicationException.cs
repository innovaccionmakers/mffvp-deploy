using Common.SharedKernel.Domain;

namespace Common.SharedKernel.Application.Exceptions;

public sealed class CommonApplicationException : Exception
{
    public CommonApplicationException(string requestName, Error? error = default, Exception? innerException = default)
        : base("Application exception", innerException)
    {
        RequestName = requestName;
        Error = error;
    }

    public string RequestName { get; }

    public Error? Error { get; }
}
