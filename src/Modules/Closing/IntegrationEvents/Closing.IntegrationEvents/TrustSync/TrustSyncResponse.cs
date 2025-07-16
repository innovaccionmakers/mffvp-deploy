namespace Closing.IntegrationEvents.TrustSync;

public sealed record TrustSyncResponse(
    bool Succeeded,
    string? Code = null,
    string? Message = null);
