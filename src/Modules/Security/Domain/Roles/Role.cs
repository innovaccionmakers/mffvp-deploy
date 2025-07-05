using Common.SharedKernel.Domain;

using Security.Domain.RolePermissions;

namespace Security.Domain.Roles;

public sealed class Role : Entity
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Objective { get; private set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    private Role() { }

    public static Result<Role> Create(int id, string name, string objective)
    {
        var role = new Role
        {
            Id = id,
            Name = name,
            Objective = objective
        };

        return Result.Success(role);
    }

    public void Update(string name, string objective)
    {
        Name = name;
        Objective = objective;
    }
}
