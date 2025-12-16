using System.Text.Json.Serialization;

namespace Security.Application.Contracts.Permissions;

[JsonConverter(typeof(PermissionDtoJsonConverter))]
public abstract class PermissionDtoBase
{
    public Guid PermissionId { get; init; }
    public string ScopePermission { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public string Description { get; init; } = default!;
}

