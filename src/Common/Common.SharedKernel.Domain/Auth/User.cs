namespace Common.SharedKernel.Domain.Auth;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
}