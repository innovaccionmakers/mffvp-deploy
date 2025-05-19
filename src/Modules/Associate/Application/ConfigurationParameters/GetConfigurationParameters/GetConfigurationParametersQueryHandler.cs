using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Associate.Domain.ConfigurationParameters;
using Associate.Integrations.ConfigurationParameters.GetConfigurationParameters;
using Associate.Integrations.ConfigurationParameters;
using System.Collections.Generic;
using System.Linq;

namespace Associate.Application.ConfigurationParameters.GetConfigurationParameters;

internal sealed class GetConfigurationParametersQueryHandler(
    IConfigurationParameterRepository configurationparameterRepository)
    : IQueryHandler<GetConfigurationParametersQuery, IReadOnlyCollection<ConfigurationParameterResponse>>
{
    public async Task<Result<IReadOnlyCollection<ConfigurationParameterResponse>>> Handle(GetConfigurationParametersQuery request, CancellationToken cancellationToken)
    {
        var entities = await configurationparameterRepository.GetAllAsync(cancellationToken);
        
        var response = entities
            .Select(e => new ConfigurationParameterResponse(
                e.ConfigurationParameterId,
                e.Uuid,
                e.Name,
                e.ParentId,
                e.Parent,
                e.Children,
                e.Status,
                e.Type,
                e.Editable,
                e.System,
                e.Metadata,
                e.HomologationCode))
            .ToList();

        return Result.Success<IReadOnlyCollection<ConfigurationParameterResponse>>(response);
    }
}