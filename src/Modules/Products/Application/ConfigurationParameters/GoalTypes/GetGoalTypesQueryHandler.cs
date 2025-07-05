using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.ConfigurationParameters;
using Products.Integrations.ConfigurationParameters;
using Products.Integrations.ConfigurationParameters.GoalTypes;

namespace Products.Application.ConfigurationParameters.GoalTypes;

public class GetGoalTypesQueryHandler(IConfigurationParameterRepository repository) : IQueryHandler<GetGoalTypesQuery, IReadOnlyCollection<ConfigurationParameterResponse>>
{
    public async Task<Result<IReadOnlyCollection<ConfigurationParameterResponse>>> Handle(GetGoalTypesQuery request, CancellationToken cancellationToken)
    {
        var list = await repository.GetActiveConfigurationParametersByTypeAsync(ConfigurationParameterType.TipoObjetivo, cancellationToken);

        var response = list
            .Select(e => new ConfigurationParameterResponse(
                e.ConfigurationParameterId,
                e.Uuid,
                e.Name,
                e.ParentId,
                e.Status,
                e.Type,
                e.Editable,
                e.System,
                e.Metadata,
                e.HomologationCode
            ))
            .ToList();

        return Result.Success<IReadOnlyCollection<ConfigurationParameterResponse>>(response);
    }
}
