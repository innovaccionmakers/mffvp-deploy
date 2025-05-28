namespace Common.SharedKernel.Domain.Auth;

public class UserPermission
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string PermitToken { get; set; } = default!;
    public bool Granted { get; set; }
}
