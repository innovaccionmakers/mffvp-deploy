using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

using Security.Application.Contracts.RolePermissions;
using Security.Domain.RolePermissions;

namespace Security.Application.RolePermissions;

public sealed class GetPermissionsByRoleIdQueryHandler(
    IRolePermissionRepository repository)
    : IQueryHandler<GetPermissionsByRoleIdQuery, IReadOnlyCollection<string>>
{
    public async Task<Result<IReadOnlyCollection<string>>> Handle(
        GetPermissionsByRoleIdQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await repository.GetPermissionsByRoleIdAsync(request.RoleId, cancellationToken);

        return Result.Success<IReadOnlyCollection<string>>(permissions);
    }
}