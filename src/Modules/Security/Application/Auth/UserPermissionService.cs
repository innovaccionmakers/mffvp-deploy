using Security.Application.Contracts.Auth;
using Security.Domain.RolePermissions;
using Security.Domain.UserRoles;
using Security.Domain.Users;

namespace Security.Application.Auth;

public class UserPermissionService : IUserPermissionService
{
    private readonly IUserRoleRepository _userRoleRepo;
    private readonly IRolePermissionRepository _rolePermissionRepo;
    private readonly IUserRepository _userRepo;

    public UserPermissionService(
        IUserRoleRepository userRoleRepo,
        IRolePermissionRepository rolePermissionRepo,
        IUserRepository userRepo)
    {
        _userRoleRepo = userRoleRepo;
        _rolePermissionRepo = rolePermissionRepo;
        _userRepo = userRepo;
    }

    public async Task<List<string>> GetPermissionsAsync(int userId)
    {
        var roleIds = await _userRoleRepo.GetRoleIdsByUserIdAsync(userId);
        var permissionsFromRoles = await _rolePermissionRepo.GetPermissionsByRoleIdsAsync(roleIds);
        return permissionsFromRoles;
    }

    public async Task<List<string>> GetPermissionsByUserNameAsync(string userName)
    {
        var user = await _userRepo.GetByUserNameAsync(userName);
        if (user == null)
            return new List<string>();

        var roleIds = await _userRoleRepo.GetRoleIdsByUserIdAsync(user.Id);
        var permissionsFromRoles = await _rolePermissionRepo.GetPermissionsByRoleIdsAsync(roleIds);
        return permissionsFromRoles;
    }
}
