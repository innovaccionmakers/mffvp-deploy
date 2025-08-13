using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.OperationTypes;
using Operations.Integrations.OperationTypes;

namespace Operations.Application.OperationTypes;

public class GetOperationTypesByCategoryQueryHandler(IOperationTypeRepository repository) : IQueryHandler<GetOperationTypesByCategoryQuery, IReadOnlyCollection<OperationType>>
{
    public async Task<Result<IReadOnlyCollection<OperationType>>> Handle(GetOperationTypesByCategoryQuery request, CancellationToken cancellationToken)
    {
        var list = await repository.GetCategoryIdAsync(request.categoryId, cancellationToken);
        return Result.Success(list);
    }
}
