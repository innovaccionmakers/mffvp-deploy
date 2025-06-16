using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.TransactionTypes;
using Operations.Integrations.TransactionTypes;

namespace Operations.Application.GetConfigurationParameters;

public class GetTransactionTypesQueryHandler(
    ITransactionTypeRepository repository
    ) : IQueryHandler<GetTransactionTypesQuery, IReadOnlyCollection<TransactionType>>
{
    const string ConfigurationParameterType = "TipoTransaccion";
    public async Task<Result<IReadOnlyCollection<TransactionType>>> Handle(GetTransactionTypesQuery request, CancellationToken cancellationToken)
    {
        var list = await repository.GetTransactionTypesByTypeAsync(ConfigurationParameterType, cancellationToken);

        return Result.Success(list);
    }
}
