using Common.SharedKernel.Domain.Auth.Permissions;

namespace Security.Application.Contracts.Permissions;

public static class PermissionDtoFactory
{
    public static PermissionDtoBase CreateFrom(MakersPermissionBase permission)
    {
        return permission switch
        {
            MakersPermissionWithSubResource subResourcePermission => new PermissionWithSubResourceDto
            {
                PermissionId = subResourcePermission.PermissionId,
                ScopePermission = subResourcePermission.ScopePermission,
                DisplayName = subResourcePermission.DisplayName,
                Description = subResourcePermission.Description,
                SubResource = subResourcePermission.SubResource,
                DisplaySubResource = subResourcePermission.DisplaySubResource
            },
            _ => new PermissionDto
            {
                PermissionId = permission.PermissionId,
                ScopePermission = permission.ScopePermission,
                DisplayName = permission.DisplayName,
                Description = permission.Description
            }
        };
    }
}

