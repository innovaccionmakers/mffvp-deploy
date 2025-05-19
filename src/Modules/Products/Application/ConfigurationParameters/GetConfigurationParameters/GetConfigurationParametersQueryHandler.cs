using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.ConfigurationParameters;
using Products.Integrations.ConfigurationParameters;
using Products.Integrations.ConfigurationParameters.GetConfigurationParameters;

namespace Products.Application.ConfigurationParameters.GetConfigurationParameters;

internal sealed class GetConfigurationParametersQueryHandler(
    IConfigurationParameterRepository repository
) : IQueryHandler<GetConfigurationParametersQuery, IReadOnlyCollection<ConfigurationParameterResponse>>
{
    public async Task<Result<IReadOnlyCollection<ConfigurationParameterResponse>>> Handle(
        GetConfigurationParametersQuery request,
        CancellationToken cancellationToken)
    {
        var list = await repository.GetAllAsync(cancellationToken);

        var response = list
            .Select(p => new ConfigurationParameterResponse(
                p.ConfigurationParameterId,
                p.Uuid,
                p.Name,
                p.ParentId,
                p.Status,
                p.Type,
                p.Editable,
                p.System,
                p.Metadata,
                p.HomologationCode
            ))
            .ToList();

        return Result.Success<IReadOnlyCollection<ConfigurationParameterResponse>>(response);
    }
}