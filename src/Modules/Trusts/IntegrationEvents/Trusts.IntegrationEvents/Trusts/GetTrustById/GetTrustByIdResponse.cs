namespace Trusts.IntegrationEvents.Trusts.GetTrustById;

public sealed record GetTrustByIdResponse(
    bool Succeeded,
    string? Code,
    string? Message,
    TrustDetails? Trust);
