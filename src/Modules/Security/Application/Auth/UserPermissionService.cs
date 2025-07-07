using Security.Application.Contracts.Auth;
using Security.Domain.RolePermissions;
using Security.Domain.UserPermissions;
using Security.Domain.UserRoles;

namespace Security.Application.Auth;

public class UserPermissionService : IUserPermissionService
{
    private readonly IUserPermissionRepository _userPermissionRepo;
    private readonly IUserRoleRepository _userRoleRepo;
    private readonly IRolePermissionRepository _rolePermissionRepo;

    public UserPermissionService(
        IUserPermissionRepository userPermissionRepo,
        IUserRoleRepository userRoleRepo,
        IRolePermissionRepository rolePermissionRepo)
    {
        _userPermissionRepo = userPermissionRepo;
        _userRoleRepo = userRoleRepo;
        _rolePermissionRepo = rolePermissionRepo;
    }

    public async Task<List<string>> GetPermissionsAsync(int userId)
    {
        //var direct = await _userPermissionRepo.GetGrantedPermissionsByUserIdAsync(userId);
        var roleIds = await _userRoleRepo.GetRoleIdsByUserIdAsync(userId);
        var permissionsFromRoles = await _rolePermissionRepo.GetPermissionsByRoleIdsAsync(roleIds);
        return permissionsFromRoles;
    }
}
