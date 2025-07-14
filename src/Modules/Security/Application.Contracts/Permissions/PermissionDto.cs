namespace Security.Application.Contracts.Permissions;

public sealed class PermissionDto
{
    public string ScopePermission { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public string Description { get; init; } = default!;
}
