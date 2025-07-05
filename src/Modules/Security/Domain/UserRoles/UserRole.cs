using Common.SharedKernel.Domain;

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

    public static Result<UserRole> Create(int rolePermissionsId, int userId)
    {
        if (rolePermissionsId <= 0)
        {
            return Result.Failure<UserRole>(Error.Validation(
                "UserRole.InvalidRolePermissionId",
                "The RolePermissionsId must be greater than zero."));
        }

        if (userId <= 0)
        {
            return Result.Failure<UserRole>(Error.Validation(
                "UserRole.InvalidUserId",
                "The UserId must be greater than zero."));
        }

        var userRole = new UserRole
        {
            Id = default,
            RolePermissionsId = rolePermissionsId,
            UserId = userId
        };

        return Result.Success(userRole);
    }
}