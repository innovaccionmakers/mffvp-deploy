using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Associate.Domain.ConfigurationParameters;
using Associate.Integrations.ConfigurationParameters.GetConfigurationParameter;
using Associate.Integrations.ConfigurationParameters;

namespace Associate.Application.ConfigurationParameters.GetConfigurationParameter;

internal sealed class GetConfigurationParameterQueryHandler(
    IConfigurationParameterRepository configurationparameterRepository)
    : IQueryHandler<GetConfigurationParameterQuery, ConfigurationParameterResponse>
{
    public async Task<Result<ConfigurationParameterResponse>> Handle(GetConfigurationParameterQuery request, CancellationToken cancellationToken)
    {
        var configurationparameter = await configurationparameterRepository.GetAsync(request.ConfigurationParameterId, cancellationToken);
        if (configurationparameter is null)
        {
            return Result.Failure<ConfigurationParameterResponse>(ConfigurationParameterErrors.NotFound(request.ConfigurationParameterId));
        }
        var response = new ConfigurationParameterResponse(
            configurationparameter.ConfigurationParameterId,
            configurationparameter.Uuid,
            configurationparameter.Name,
            configurationparameter.ParentId,
            configurationparameter.Parent,
            configurationparameter.Children,
            configurationparameter.Status,
            configurationparameter.Type,
            configurationparameter.Editable,
            configurationparameter.System,
            configurationparameter.Metadata,
            configurationparameter.HomologationCode
        );
        return response;
    }
}