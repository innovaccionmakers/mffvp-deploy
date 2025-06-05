namespace Common.SharedKernel.Application.Auth;

public class UserPermissionService : IUserPermissionService
{
    public Task<List<string>> GetPermissionsAsync(int userId)
    {
        var direct = SeedData.UserPermissions
            .Where(p => p.UserId == userId && p.Granted)
            .Select(p => p.PermitToken);

        var roleIds = SeedData.UserRoles
            .Where(r => r.UserId == userId)
            .Select(r => r.RoleId);

        var fromRoles = SeedData.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.PermitToken);

        var all = direct.Concat(fromRoles).Distinct().ToList();
        return Task.FromResult(all);
    }
}