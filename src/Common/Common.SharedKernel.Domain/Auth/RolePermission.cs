namespace Common.SharedKernel.Domain.Auth;

public class RolePermission
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string PermitToken { get; set; } = default!;
}
