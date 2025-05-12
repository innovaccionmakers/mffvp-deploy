namespace Activations.Integrations.MeetsPensionRequirements;

public sealed record MeetsPensionRequirementResponse(
    int MeetsPensionRequirementId,
    int AffiliateId,
    DateTime StartDate,
    DateTime ExpirationDate,
    DateTime CreationDate,
    string State
);