using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.Origins;
using Operations.Integrations.Origins;

namespace Operations.Application.Origins;

public class GetOriginContributionsQueryHandler(IOriginRepository repository) : IQueryHandler<GetOriginContributionsQuery, IReadOnlyCollection<Origin>>
{
    public async Task<Result<IReadOnlyCollection<Origin>>> Handle(GetOriginContributionsQuery request, CancellationToken cancellationToken)
    {
        var list = await repository.GetOriginsAsync(cancellationToken);
        return Result.Success(list);
    }
}
