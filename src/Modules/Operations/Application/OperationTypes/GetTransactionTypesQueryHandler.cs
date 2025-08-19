using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.OperationTypes;
using Operations.Integrations.OperationTypes;

namespace Operations.Application.OperationTypes;

public class GetTransactionTypesQueryHandler(
    IOperationTypeRepository repository
) : IQueryHandler<GetTransactionTypesQuery, IReadOnlyCollection<OperationType>>
{
    public async Task<Result<IReadOnlyCollection<OperationType>>> Handle(GetTransactionTypesQuery request, CancellationToken cancellationToken)
    {
        var list = await repository.GetCategoryIdAsync(null, cancellationToken);
        return Result.Success(list);
    }
}