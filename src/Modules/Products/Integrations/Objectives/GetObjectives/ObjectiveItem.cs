using System.Text.Json.Serialization;

namespace Products.Integrations.Objectives.GetObjectives;

public sealed record ObjectiveItem(
    [property: JsonPropertyName("Objetivo")] 
    ObjectiveDto Objective
);