
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.OperationTypes;

namespace Closing.Application.Abstractions.External.Operations.OperationTypes
{
    public interface IOperationTypesLocator
    {
        Task<Result<IReadOnlyCollection<OperationTypeInfo>>> GetAllOperationTypesAsync(
            CancellationToken cancellationToken);
        Task<Result<OperationTypeInfo>> GetOperationTypeByNameAsync(
             string name,
             CancellationToken cancellationToken);

    }

    public sealed record OperationTypeInfo(
        long OperationTypeId,
        string Name,
        string? Category,
        IncomeEgressNature Nature,
        Status Status,
        string External,
        string HomologatedCode
    );

}
