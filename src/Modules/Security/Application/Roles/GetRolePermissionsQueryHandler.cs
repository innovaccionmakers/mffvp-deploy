using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

using Security.Application.Contracts.Roles;
using Security.Domain.RolePermissions;

namespace Security.Application.Roles;

public sealed class GetRolePermissionsQueryHandler(IRolePermissionRepository repository)
    : IQueryHandler<GetRolePermissionsQuery, IReadOnlyCollection<RolePermission>>
{
    public async Task<Result<IReadOnlyCollection<RolePermission>>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
    {
        var all = await repository.GetAllAsync(cancellationToken);
        var filtered = all.Where(p => p.RolesId == request.RoleId).ToList();

        return Result.Success<IReadOnlyCollection<RolePermission>>(filtered);
    }
}
