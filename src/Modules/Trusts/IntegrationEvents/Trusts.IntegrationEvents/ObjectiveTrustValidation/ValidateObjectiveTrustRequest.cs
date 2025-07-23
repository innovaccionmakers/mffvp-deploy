namespace Trusts.IntegrationEvents.ObjectiveTrustValidation;

public sealed record ValidateObjectiveTrustRequest(
    int ObjectiveId,
    string RequestedStatus
);