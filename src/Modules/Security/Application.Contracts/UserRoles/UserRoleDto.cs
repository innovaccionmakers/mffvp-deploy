namespace Security.Application.Contracts.UserRoles;

public sealed class UserRoleDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}