namespace Products.Integrations.Objectives.GetObjectives;

public sealed record GetObjectivesResponse(
    IReadOnlyList<ObjectiveDto> Objectives
);