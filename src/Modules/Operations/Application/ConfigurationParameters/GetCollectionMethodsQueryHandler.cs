using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.ConfigurationParameters;
using Operations.Integrations.ConfigurationParameters;

namespace Operations.Application.ConfigurationParameters;

public class GetCollectionMethodsQueryHandler(
    IConfigurationParameterRepository repository
    ) : IQueryHandler<GetCollectionMethodsQuery, IReadOnlyCollection<ConfigurationParameterResponse>>
{
    public async Task<Result<IReadOnlyCollection<ConfigurationParameterResponse>>> Handle(GetCollectionMethodsQuery request, CancellationToken cancellationToken)
    {
        
        var list = await repository.GetActiveConfigurationParametersByTypeAsync(ConfigurationParameterType.MetodoRecaudo, cancellationToken);

        var response = list
            .Select(e => new ConfigurationParameterResponse(
                e.ConfigurationParameterId.ToString(),
                e.Name,
                e.HomologationCode,
                e.Status))
            .ToList();

        return Result.Success<IReadOnlyCollection<ConfigurationParameterResponse>>(response);
    }
}
