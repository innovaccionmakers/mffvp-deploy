namespace Security.Domain.RolePermissions;

public class RolePermissionDto
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string ScopePermission { get; set; }
    public string DisplayName { get; set; }
}
