
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.SubtransactionTypes;

namespace Closing.Application.Abstractions.External.Operations.OperationTypes
{
    public interface IOperationTypesLocator
    {
        Task<Result<IReadOnlyCollection<OperationTypesRemoteResponse>>> GetAllOperationTypesAsync(
            CancellationToken cancellationToken);
    }

    public sealed record OperationTypesRemoteResponse(
        long OperationTypeId,
        string Name,
        string? Category,
        IncomeEgressNature Nature,
        Status Status
    );
}
