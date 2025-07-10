using Operations.Domain.ClientOperations;

namespace Operations.Application.Abstractions.Services.OperationCompleted;

public interface IOperationCompleted
{
    Task ExecuteAsync(ClientOperation operation, CancellationToken cancellationToken);
}