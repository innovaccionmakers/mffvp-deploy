namespace Closing.IntegrationEvents.DataSync.TrustSync;

public sealed record TrustSyncResponse(
    bool Succeeded,
    string? Code = null,
    string? Message = null);
