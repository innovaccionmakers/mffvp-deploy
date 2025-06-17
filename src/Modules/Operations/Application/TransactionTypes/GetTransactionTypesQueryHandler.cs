using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.TransactionTypes;
using Operations.Integrations.TransactionTypes;

namespace Operations.Application.TransactionTypes;

public class GetTransactionTypesQueryHandler(
    ITransactionTypeRepository repository
    ) : IQueryHandler<GetTransactionTypesQuery, IReadOnlyCollection<TransactionType>>
{    
    public async Task<Result<IReadOnlyCollection<TransactionType>>> Handle(GetTransactionTypesQuery request, CancellationToken cancellationToken)
    {
        var list = await repository.GetTransactionTypesAsync(cancellationToken);

        return Result.Success(list);
    }
}
