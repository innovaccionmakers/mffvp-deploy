namespace Treasury.IntegrationEvents.Issuers.GetIssuersByIds;

public sealed record GetIssuersByIdsRequest(
    IEnumerable<long> Ids
);

