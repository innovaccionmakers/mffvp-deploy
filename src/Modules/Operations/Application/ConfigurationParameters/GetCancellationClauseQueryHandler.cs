using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.ConfigurationParameters;
using Operations.Integrations.ConfigurationParameters;

namespace Operations.Application.ConfigurationParameters;

public class GetCancellationClauseQueryHandler(
    IConfigurationParameterRepository repository
) : IQueryHandler<GetCancellationClauseQuery, IReadOnlyCollection<CancellationClauseConfigurationParameterResponse>>
{
    public async Task<Result<IReadOnlyCollection<CancellationClauseConfigurationParameterResponse>>> Handle(
        GetCancellationClauseQuery request,
        CancellationToken cancellationToken
    )
    {
        var clauses = await repository.GetActiveConfigurationParametersByTypeAsync(
            ConfigurationParameterType.CasualAnulacion,
            cancellationToken
        );

        var response = clauses
            .Select(configurationParameter => new CancellationClauseConfigurationParameterResponse(
                configurationParameter.ConfigurationParameterId.ToString(),
                configurationParameter.Uuid,
                configurationParameter.Name,
                configurationParameter.HomologationCode,
                configurationParameter.Status
            ))
            .ToList();

        return Result.Success<IReadOnlyCollection<CancellationClauseConfigurationParameterResponse>>(response);
    }
}
