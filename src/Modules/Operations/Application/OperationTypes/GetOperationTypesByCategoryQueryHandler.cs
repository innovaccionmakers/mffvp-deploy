using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.OperationTypes;
using Operations.Integrations.OperationTypes;
using System.Linq;

namespace Operations.Application.OperationTypes;

public class GetOperationTypesByCategoryQueryHandler(IOperationTypeRepository repository)
    : IQueryHandler<GetOperationTypesByCategoryQuery, IReadOnlyCollection<OperationType>>
{
    public async Task<Result<IReadOnlyCollection<OperationType>>> Handle(
        GetOperationTypesByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<OperationType> list;

        if (request.categoryId.HasValue)
        {
            list = await repository.GetTypesByCategoryAsync(request.categoryId, cancellationToken);
        }
        else
        {
            list = (await repository.GetAllAsync(cancellationToken))
                .Where(x => x.CategoryId.HasValue)
                .ToList();
        }

        return Result.Success(list);
    }
}