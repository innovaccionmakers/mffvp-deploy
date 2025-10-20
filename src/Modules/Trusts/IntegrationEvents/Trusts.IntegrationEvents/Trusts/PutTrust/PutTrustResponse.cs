namespace Trusts.IntegrationEvents.Trusts.PutTrust;

public sealed record PutTrustResponse(bool Succeeded, string? Code, string? Message);
