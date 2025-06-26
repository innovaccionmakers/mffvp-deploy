using Operations.Domain.ConfigurationParameters;
using Common.SharedKernel.Domain;
using Operations.Integrations.ConfigurationParameters;
using Common.SharedKernel.Application.Messaging;

namespace Operations.Application.ConfigurationParameters;

public class GetOriginModesQueryHandler(IConfigurationParameterRepository repository) : IQueryHandler<GetOriginModesQuery, IReadOnlyCollection<ConfigurationParameterResponse>>
{
    public async Task<Result<IReadOnlyCollection<ConfigurationParameterResponse>>> Handle(GetOriginModesQuery request, CancellationToken cancellationToken)
    {
        var list = await repository.GetActiveConfigurationParametersByTypeAsync(ConfigurationParameterType.ModalidadOrigen, cancellationToken);

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