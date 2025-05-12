namespace Activations.Integrations.Affiliates;

public sealed record AffiliateResponse(
    int AffiliateId,
    string IdentificationType,
    string Identification,
    bool Pensioner,
    bool MeetsRequirements,
    DateTime ActivationDate
);