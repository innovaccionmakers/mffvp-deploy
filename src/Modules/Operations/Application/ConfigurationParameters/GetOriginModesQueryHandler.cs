using Operations.Domain.ConfigurationParameters;
using Common.SharedKernel.Domain;
using Operations.Integrations.ConfigurationParameters;
using Common.SharedKernel.Application.Messaging;

namespace Operations.Application.ConfigurationParameters;

public class GetOriginModesQueryHandler(IConfigurationParameterRepository repository) : IQueryHandler<GetOriginModesQuery, IReadOnlyCollection<OriginMode>>
{
    public async Task<Result<IReadOnlyCollection<OriginMode>>> Handle(GetOriginModesQuery request, CancellationToken cancellationToken)
    {
        var list = await repository.GetOriginModesAsync(cancellationToken);
        return Result.Success(list);
    }
}