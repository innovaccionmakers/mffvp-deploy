using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.ConfigurationParameters;
using Operations.Integrations.ConfigurationParameters;

namespace Operations.Application.ConfigurationParameters;

public class GetDocumentTypesQueryHandler(IConfigurationParameterRepository repository) : IQueryHandler<GetDocumentTypesQuery, IReadOnlyCollection<ConfigurationParameterResponse>>
{
    public async Task<Result<IReadOnlyCollection<ConfigurationParameterResponse>>> Handle(GetDocumentTypesQuery request, CancellationToken cancellationToken)
    {
        var list = await repository.GetActiveConfigurationParametersByTypeAsync(ConfigurationParameterType.TipoDocumento, cancellationToken);

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
