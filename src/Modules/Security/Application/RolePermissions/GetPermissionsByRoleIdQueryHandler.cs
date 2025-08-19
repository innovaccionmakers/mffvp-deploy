using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

using MediatR;

using Security.Application.Contracts.Permissions;
using Security.Application.Contracts.RolePermissions;
using Security.Domain.RolePermissions;
using Security.Domain.Roles;

namespace Security.Application.RolePermissions;

public sealed class GetPermissionsByRoleIdQueryHandler(
    IRolePermissionRepository repository,
    IRoleRepository roleRepository,
    ISender sender)
    : IQueryHandler<GetPermissionsByRoleIdQuery, IReadOnlyCollection<RolePermissionDto>>
{
    public async Task<Result<IReadOnlyCollection<RolePermissionDto>>> Handle(
        GetPermissionsByRoleIdQuery request,
        CancellationToken cancellationToken)
    {

        var roleExists = await roleRepository.ExistsAsync(request.RoleId, cancellationToken);
        if (!roleExists)
        {
            return Result.Failure<IReadOnlyCollection<RolePermissionDto>>(Error.NotFound(
                "Role.NotFound",
                "The specified role does not exist."));
        }

        var existingPermissions = await repository.GetPermissionsByRoleIdAsync(request.RoleId, cancellationToken);

        var allPermissionsResult = await sender.Send(new GetAllPermissionsQuery(), cancellationToken);
        if (allPermissionsResult.IsFailure)
            return Result.Failure<IReadOnlyCollection<RolePermissionDto>>(allPermissionsResult.Error);

        var allDefinedPermissions = allPermissionsResult.Value;

        var existingMap = existingPermissions.ToDictionary(p => p.ScopePermission, p => p);

        var permissionsMerged = allDefinedPermissions
            .Select(defined =>
            {
                if (existingMap.TryGetValue(defined.ScopePermission, out var existing))
                {
                    return new RolePermissionDto
                    {
                        Id = existing.Id,
                        RoleId = existing.RoleId,
                        ScopePermission = existing.ScopePermission,
                        DisplayName = defined.DisplayName
                    };
                }

                return new RolePermissionDto
                {
                    Id = 0,
                    RoleId = request.RoleId,
                    ScopePermission = defined.ScopePermission,
                    DisplayName = defined.DisplayName
                };
            })
            .ToList();

        return Result.Success<IReadOnlyCollection<RolePermissionDto>>(permissionsMerged);
    }
}