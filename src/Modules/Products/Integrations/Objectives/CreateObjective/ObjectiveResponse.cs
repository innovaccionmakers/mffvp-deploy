using System.Text.Json.Serialization;

namespace Products.Integrations.Objectives.CreateObjective;

public sealed record ObjectiveResponse(
    [property: JsonPropertyName("IdObjetivo")]
    int? ObjectiveId
);