namespace Security.Application.Contracts.Permissions;

public sealed class PermissionWithSubResourceDto : PermissionDtoBase
{
    public string SubResource { get; init; } = default!;
    public string? DisplaySubResource { get; init; }
}

