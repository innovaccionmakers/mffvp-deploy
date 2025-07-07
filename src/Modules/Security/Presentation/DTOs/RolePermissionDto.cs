namespace Security.Presentation.DTOs;

public class RolePermissionDto
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string ScopePermission { get;  set; }
}
