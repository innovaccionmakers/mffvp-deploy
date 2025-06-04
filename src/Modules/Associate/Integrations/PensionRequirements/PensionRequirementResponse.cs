namespace Associate.Integrations.PensionRequirements;

public sealed record PensionRequirementResponse(
    int PensionRequirementId,
    int AffiliateId,
    DateTime StartDate,
    DateTime ExpirationDate,
    DateTime CreationDate,
    bool Status
);