using Common.SharedKernel.Domain;

using Security.Domain.RolePermissions;
using Security.Domain.Users;

namespace Security.Domain.UserRoles;

public sealed class UserRole : Entity
{
    public int Id { get; private set; }
    public int RolePermissionsId { get; private set; }
    public int UserId { get; private set; }

    public RolePermission RolePermission { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private UserRole() { }

    public static Result<UserRole> Create(int rolePermissionsId, int userId)
    {
        var userRole = new UserRole
        {
            Id = default,
            RolePermissionsId = rolePermissionsId,
            UserId = userId
        };

        return Result.Success(userRole);
    }
}