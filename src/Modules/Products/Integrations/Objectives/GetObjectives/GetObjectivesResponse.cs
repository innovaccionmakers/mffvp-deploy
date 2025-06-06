using System.Text.Json.Serialization;

namespace Products.Integrations.Objectives.GetObjectives;

public sealed record GetObjectivesResponse(
    [property: JsonPropertyName("Objetivos")]
    IReadOnlyList<ObjectiveDto> Objectives
);