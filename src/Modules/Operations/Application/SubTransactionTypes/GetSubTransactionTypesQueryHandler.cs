using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.SubtransactionTypes;
using Operations.Integrations.SubTransactionTypes;


namespace Operations.Application.SubTransactionTypes;

public class GetSubTransactionTypesQueryHandler(ISubtransactionTypeRepository repository) : IQueryHandler<GetSubTransactionTypesQuery, IReadOnlyCollection<SubtransactionType>>
{
    public async Task<Result<IReadOnlyCollection<SubtransactionType>>> Handle(GetSubTransactionTypesQuery request, CancellationToken cancellationToken)
    {
        var list = await repository.GetCategoryIdAsync(request.categoryId, cancellationToken);
        return Result.Success(list);
    }
}
