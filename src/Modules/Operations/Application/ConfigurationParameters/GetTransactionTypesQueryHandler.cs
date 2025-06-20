using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.ConfigurationParameters;
using Operations.Integrations.ConfigurationParameters;

namespace Operations.Application.ConfigurationParameters;

public class GetTransactionTypesQueryHandler(
    IConfigurationParameterRepository repository
    ) : IQueryHandler<GetTransactionTypesQuery, IReadOnlyCollection<TransactionType>>
{    
    public async Task<Result<IReadOnlyCollection<TransactionType>>> Handle(GetTransactionTypesQuery request, CancellationToken cancellationToken)
    {
        var list = await repository.GetTransactionTypesAsync(cancellationToken);

        return Result.Success(list);
    }
}
