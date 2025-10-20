namespace Trusts.IntegrationEvents.TrustInfo;

public sealed record TrustInfoResponse(bool Succeeded, string? Code, string? Message, long? TrustId);
