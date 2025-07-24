
using Trusts.Integrations.DataSync.TrustSync.Response;

namespace Trusts.IntegrationEvents.DataSync.TrustSync;

public sealed record ActiveTrustsByPortfolioResponse(
    bool Success,
    string? ErrorCode,
    string? ErrorMessage,
    IEnumerable<GetActiveTrustByPortfolioResponse> Trusts);