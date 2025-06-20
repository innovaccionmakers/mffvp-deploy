using Operations.Domain.ConfigurationParameters;
using Common.SharedKernel.Domain;
using Operations.Integrations.ConfigurationParameters;
using Common.SharedKernel.Application.Messaging;

namespace Operations.Application.ConfigurationParameters;

public class GetCertificationStatusesQueryHandler(IConfigurationParameterRepository repository) : IQueryHandler<GetCertificationStatusesQuery, IReadOnlyCollection<ConfigurationParameterResponse>>
{
    public async Task<Result<IReadOnlyCollection<ConfigurationParameterResponse>>> Handle(GetCertificationStatusesQuery request, CancellationToken cancellationToken)
    {
        var list = await repository.GetActiveConfigurationParametersByTypeAsync(ConfigurationParameterType.EstadosCertificacion, cancellationToken);
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