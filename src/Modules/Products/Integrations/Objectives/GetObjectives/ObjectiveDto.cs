namespace Products.Integrations.Objectives.GetObjectives;

public sealed record ObjectiveDto(
    int ObjectiveId,
    string ObjectiveType,
    string ObjectiveName,
    string AlternativeId,
    string Status
);