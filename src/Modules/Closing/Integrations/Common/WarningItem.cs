using System.Text.Json.Serialization;

namespace Closing.Integrations.Common;
public sealed class WarningItem
{
    [property: JsonPropertyName("Codigo")]
    public string Code { get; init; } = default!;

    [property: JsonPropertyName("Descripcion")]
    public string Description { get; init; } = default!;

    [property: JsonPropertyName("Severidad")]
    public string Severity { get; init; } = default!; // "Leve", "Media", "Crítica"

    [property: JsonPropertyName("Prioridad")]
    public int Priority { get; init; }
}