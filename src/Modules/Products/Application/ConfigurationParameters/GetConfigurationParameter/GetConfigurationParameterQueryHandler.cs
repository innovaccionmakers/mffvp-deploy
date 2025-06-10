using Products.Domain.ConfigurationParameters;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Products.Integrations.ConfigurationParameters;
using Products.Integrations.ConfigurationParameters.GetConfigurationParameter;

namespace Products.Application.ConfigurationParameters.GetConfigurationParameter;

internal sealed class GetConfigurationParameterQueryHandler(
    IConfigurationParameterRepository repository
) : IQueryHandler<GetConfigurationParameterQuery, ConfigurationParameterResponse>
{
    public async Task<Result<ConfigurationParameterResponse>> Handle(
        GetConfigurationParameterQuery request,
        CancellationToken cancellationToken)
    {
        var parameter = await repository.GetAsync(request.ConfigurationParameterId, cancellationToken);
        if (parameter is null)
            return Result.Failure<ConfigurationParameterResponse>(
                ConfigurationParameterErrors.NotFound(request.ConfigurationParameterId));

        return new ConfigurationParameterResponse(
            parameter.ConfigurationParameterId,
            parameter.Uuid,
            parameter.Name,
            parameter.ParentId,
            parameter.Status,
            parameter.Type,
            parameter.Editable,
            parameter.System,
            parameter.Metadata,
            parameter.HomologationCode
        );
    }
}