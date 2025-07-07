using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.Auth.Permissions;

using Security.Application.Contracts.RolePermissions;
using Security.Domain.RolePermissions;

namespace Security.Application.RolePermissions;

public sealed class GetPermissionsByRoleIdQueryHandler(
    IRolePermissionRepository repository)
    : IQueryHandler<GetPermissionsByRoleIdQuery, IReadOnlyCollection<RolePermission>>
{
    public async Task<Result<IReadOnlyCollection<RolePermission>>> Handle(
        GetPermissionsByRoleIdQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await repository.GetPermissionsByRoleIdAsync(request.RoleId, cancellationToken);

        var allPermissions = MakersPermissionsOperationsAuxiliaryInformations.All
            .Concat(MakersPermissionsOperationsClientOperations.All)
            .Select(p => p.Key)
            .Distinct()
            .ToList();

        var mergedPermissions = allPermissions
            .Select(permission =>
            {
                var existing = permissions.FirstOrDefault(rp => rp.ScopePermission == permission);
                if (existing is not null)
                    return existing;

                return RolePermission.Create(0, request.RoleId, permission);
            })
            .ToList();

        return Result.Success<IReadOnlyCollection<RolePermission>>(mergedPermissions);
    }
}