using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.ConfigurationParameters;
using Operations.Integrations.ConfigurationParameters;

namespace Operations.Application.ConfigurationParameters;

public class GetDebitNoteCausesQueryHandler(
    IConfigurationParameterRepository repository
) : IQueryHandler<GetDebitNoteCausesQuery, IReadOnlyCollection<ConfigurationParameterResponse>>
{
    public async Task<Result<IReadOnlyCollection<ConfigurationParameterResponse>>> Handle(
        GetDebitNoteCausesQuery request,
        CancellationToken cancellationToken
    )
    {
        var causes = await repository.GetActiveConfigurationParametersByTypeAsync(
            ConfigurationParameterType.CausalesNotaDebito,
            cancellationToken
        );

        var response = causes
            .Select(configurationParameter => new ConfigurationParameterResponse(
                configurationParameter.ConfigurationParameterId.ToString(),
                configurationParameter.Uuid,
                configurationParameter.Name,
                configurationParameter.HomologationCode,
                configurationParameter.Status
            ))
            .ToList();

        return Result.Success<IReadOnlyCollection<ConfigurationParameterResponse>>(response);
    }
}
