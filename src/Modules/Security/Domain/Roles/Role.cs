using Security.Domain.RolePermissions;

namespace Security.Domain.Roles;

public sealed class Role
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Objective { get; private set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    private Role() { }

    public Role(int id, string name, string objective)
    {
        Id = id;
        Name = name;
        Objective = objective;
    }
}
