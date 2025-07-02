using Security.Domain.Roles;
using Security.Domain.UserRoles;

namespace Security.Domain.RolePermissions;

public sealed class RolePermission
{
    public int Id { get; private set; }
    public int RolesId { get; private set; }
    public string ScopePermission { get; private set; } = string.Empty;

    public Role Role { get; private set; } = null!;
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    private RolePermission() { }

    public RolePermission(int rolesId, string scopePermission)
    {
        RolesId = rolesId;
        ScopePermission = scopePermission;
    }
}
