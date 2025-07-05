using Common.SharedKernel.Domain;

using Security.Domain.Roles;
using Security.Domain.UserRoles;

namespace Security.Domain.RolePermissions;

public sealed class RolePermission : Entity
{
    public int Id { get; private set; }
    public int RoleId { get; private set; }
    public string ScopePermission { get; private set; } = string.Empty;

    public Role Role { get; private set; } = null!;
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    private RolePermission() { }

    public static Result<RolePermission> Create(int roleId, string scopePermission)
    {
        var rolePermission = new RolePermission
        {
            Id = default,
            RoleId = roleId,
            ScopePermission = scopePermission
        };

        return Result.Success(rolePermission);
    }
}
