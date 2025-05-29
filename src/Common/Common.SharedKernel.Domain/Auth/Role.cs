namespace Common.SharedKernel.Domain.Auth;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Objective { get; set; } = default!;
}
