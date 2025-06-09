using Trusts.Integrations.Trusts;

namespace Trusts.IntegrationEvents.CreateTrust;

public sealed record CreateTrustResponse(
    bool Succeeded,
    TrustResponse? Trust,
    string? Code,
    string? Message);