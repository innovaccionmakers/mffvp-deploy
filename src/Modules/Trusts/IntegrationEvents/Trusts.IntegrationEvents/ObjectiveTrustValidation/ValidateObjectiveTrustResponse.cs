namespace Trusts.IntegrationEvents.ObjectiveTrustValidation;

public sealed record ValidateObjectiveTrustResponse(
    bool CanUpdate,
    bool HasTrust,
    bool HasTrustWithBalance,
    string? Code = null,
    string? Message = null
);