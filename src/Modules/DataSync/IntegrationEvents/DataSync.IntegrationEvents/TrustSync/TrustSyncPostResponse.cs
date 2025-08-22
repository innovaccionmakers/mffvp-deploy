namespace DataSync.IntegrationEvents.TrustSync;

public sealed record TrustSyncPostResponse(
    bool Succeeded,
    string? Code = null,
    string? Message = null);
