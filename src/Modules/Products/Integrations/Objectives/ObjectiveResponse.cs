namespace Products.Integrations.Objectives;

public sealed record ObjectiveResponse(
    long ObjectiveId,
    int ObjectiveTypeId,
    int AffiliateId,
    int AlternativeId,
    string Name,
    string Status,
    DateTime CreationDate
);