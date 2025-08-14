using Closing.Application.Abstractions.External.Operations.OperationTypes;
using Common.SharedKernel.Domain;

namespace Closing.Application.Closing.Services.OperationTypes;

public interface IOperationTypesService
{
    Task<Result<IReadOnlyCollection<OperationTypesRemoteResponse>>> GetAllAsync(CancellationToken cancellationToken);
}
