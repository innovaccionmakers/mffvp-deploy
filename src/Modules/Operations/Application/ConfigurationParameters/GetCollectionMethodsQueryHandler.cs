using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.ConfigurationParameters;
using Operations.Integrations.ConfigurationParameters;

namespace Operations.Application.ConfigurationParameters;

public class GetCollectionMethodsQueryHandler(
    IConfigurationParameterRepository repository
    ) : IQueryHandler<GetCollectionMethodsQuery, IReadOnlyCollection<CollectionMethod>>
{
    public async Task<Result<IReadOnlyCollection<CollectionMethod>>> Handle(GetCollectionMethodsQuery request, CancellationToken cancellationToken)
    {
        
        var list = await repository.GetCollectionMethodsAsync(cancellationToken);

        return Result.Success(list);
    }
}
