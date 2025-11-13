using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.OperationTypes;
using Operations.Integrations.OperationTypes;
namespace Operations.Application.OperationTypes;

public class GetOperationTypesByCategoryQueryHandler(IOperationTypeRepository repository)
    : IQueryHandler<GetOperationTypesByCategoryQuery, IReadOnlyCollection<OperationType>>
{
    public async Task<Result<IReadOnlyCollection<OperationType>>> Handle(
        GetOperationTypesByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<OperationType> list;

        if (request.categoryId is null && request.groupLists is null && request.visible is null)
        {
            list = (await repository.GetAllAsync(cancellationToken))
                .Where(x => x.CategoryId.HasValue)
                .ToList(); 
        }
        else
        {
            list = await repository.GetTypesByCategoryAsync(request.categoryId, cancellationToken, request.groupLists, request.visible);
        }

        return Result.Success(list);
    }
}