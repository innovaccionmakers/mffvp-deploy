using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.ConfigurationParameters;
using Operations.Integrations.ConfigurationParameters;

namespace Operations.Application.ConfigurationParameters;

public class GetWithholdingContingencyQueryHandler(IConfigurationParameterRepository repository) : IQueryHandler<GetWithholdingContingencyQuery, string>
{
    public async Task<Result<string>> Handle(GetWithholdingContingencyQuery request, CancellationToken cancellationToken)
    {
        var list = await repository.GetActiveConfigurationParametersByTypeAsync(ConfigurationParameterType.PorcentajeRetencionContingente, cancellationToken);

        var withholdingContingency = list.FirstOrDefault();

        var percentage = withholdingContingency?.Metadata.RootElement.GetProperty("valor").GetString();

        return Result.Success<string>(percentage?.Replace("%", "") ?? "");
    }
}