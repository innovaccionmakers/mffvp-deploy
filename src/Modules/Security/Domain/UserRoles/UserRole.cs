using Common.SharedKernel.Domain;

using Security.Domain.RolePermissions;
using Security.Domain.Roles;
using Security.Domain.Users;

namespace Security.Domain.UserRoles;

public sealed class UserRole : Entity
{
    public int Id { get; private set; }
    public int RoleId { get; private set; }
    public int UserId { get; private set; }

    public Role Role { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private UserRole() { }

    public static Result<UserRole> Create(int roleId, int userId)
    {
        var userRole = new UserRole
        {
            RoleId = roleId,
            UserId = userId
        };

        return Result.Success(userRole);
    }
}