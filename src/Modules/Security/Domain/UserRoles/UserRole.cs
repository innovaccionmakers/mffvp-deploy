using Security.Domain.RolePermissions;
using Security.Domain.Users;

namespace Security.Domain.UserRoles;

public sealed class UserRole
{
    public int Id { get; private set; }
    public int RolePermissionsId { get; private set; }
    public int UserId { get; private set; }

    public RolePermission RolePermission { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private UserRole() { }

    public UserRole(int rolePermissionsId, int userId)
    {
        RolePermissionsId = rolePermissionsId;
        UserId = userId;
    }
}
