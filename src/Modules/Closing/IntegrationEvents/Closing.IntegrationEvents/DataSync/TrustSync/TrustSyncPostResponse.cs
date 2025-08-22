namespace Closing.IntegrationEvents.DataSync.TrustSync;
public sealed record TrustSyncPostResponse(
    bool Succeeded,
    string? Code = null,
    string? Message = null);
