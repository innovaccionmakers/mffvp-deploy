using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.ConfigurationParameters;
using Operations.Integrations.ConfigurationParameters;

namespace Operations.Application.ConfigurationParameters;

public class GetTransactionTypesQueryHandler(
    IConfigurationParameterRepository repository
    ) : IQueryHandler<GetTransactionTypesQuery, IReadOnlyCollection<ConfigurationParameterResponse>>
{    
    public async Task<Result<IReadOnlyCollection<ConfigurationParameterResponse>>> Handle(GetTransactionTypesQuery request, CancellationToken cancellationToken)
    {
        var list = await repository.GetActiveConfigurationParametersByTypeAsync(ConfigurationParameterType.TipoTransaccion, cancellationToken);

        var response = list
            .Select(e => new ConfigurationParameterResponse(
                e.ConfigurationParameterId.ToString(),
                e.Uuid,
                e.Name,
                e.HomologationCode,
                e.Status))
            .ToList();

        return Result.Success<IReadOnlyCollection<ConfigurationParameterResponse>>(response);
    }
}
