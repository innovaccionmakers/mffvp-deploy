using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

using Security.Application.Contracts.Roles;
using Security.Domain.Roles;

namespace Security.Application.Roles;

public sealed class GetRolesQueryHandler(IRoleRepository repository)
    : IQueryHandler<GetRolesQuery, IReadOnlyCollection<Role>>
{
    public async Task<Result<IReadOnlyCollection<Role>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var result = await repository.GetAllAsync(cancellationToken);
        return Result.Success(result);
    }
}