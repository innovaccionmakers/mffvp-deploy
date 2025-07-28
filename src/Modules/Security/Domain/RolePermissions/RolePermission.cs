using Common.SharedKernel.Domain;

using Security.Domain.Roles;

namespace Security.Domain.RolePermissions;

public sealed class RolePermission : Entity
{
    public int Id { get; private set; }
    public int RoleId { get; private set; }
    public string ScopePermission { get; private set; } = string.Empty;

    public Role Role { get; private set; } = null!;

    private RolePermission() { }

    public static Result<RolePermission> Create(int roleId, string scopePermission)
    {
        var rolePermission = new RolePermission
        {
            RoleId = roleId,
            ScopePermission = scopePermission
        };

        return Result.Success(rolePermission);
    }

    public static RolePermission Create(int id, int roleId, string scopePermission)
    {
        var rolePermission = new RolePermission
        {
            Id = id,
            RoleId = roleId,
            ScopePermission = scopePermission
        };

        return rolePermission;
    }
}
