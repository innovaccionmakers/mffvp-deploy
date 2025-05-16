namespace Products.Integrations.Alternatives;

public sealed record AlternativeResponse(
    long AlternativeId,
    int AlternativeTypeId,
    string Name,
    string Status,
    string Description
);