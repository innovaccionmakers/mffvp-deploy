namespace Common.SharedKernel.Application.Auth;

public static class PermissionEvaluator
{
    public static bool HasPermission(int userId, string token)
    {
        var direct = SeedData.UserPermissions.FirstOrDefault(p => p.UserId == userId && p.PermitToken == token);
        if (direct != null)
            return direct.Granted;

        var roles = SeedData.UserRoles.Where(r => r.UserId == userId).Select(r => r.RoleId).ToList();
        return SeedData.RolePermissions.Any(p => roles.Contains(p.RoleId) && p.PermitToken == token);
    }

    public static bool HasPermission(string userName, string token)
    {
        var user = SeedData.Users.FirstOrDefault(x => x.UserName == userName);
        if (user == null)
            return false;
        var direct = SeedData.UserPermissions.FirstOrDefault(p => p.UserId == user.Id && p.PermitToken == token);
        if (direct != null)
            return direct.Granted;

        var roles = SeedData.UserRoles.Where(r => r.UserId == user.Id).Select(r => r.RoleId).ToList();
        return SeedData.RolePermissions.Any(p => roles.Contains(p.RoleId) && p.PermitToken == token);
    }
}