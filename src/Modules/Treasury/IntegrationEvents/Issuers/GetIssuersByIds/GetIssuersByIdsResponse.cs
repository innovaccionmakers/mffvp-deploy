namespace Treasury.IntegrationEvents.Issuers.GetIssuersByIds;

public sealed record GetIssuersByIdsResponse(
    bool IsValid,
    string? Code,
    string? Message,
    IReadOnlyCollection<IssuerResponse> Issuers
);

