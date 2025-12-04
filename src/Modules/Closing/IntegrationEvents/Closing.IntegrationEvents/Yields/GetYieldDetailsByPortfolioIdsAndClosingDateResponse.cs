using Closing.Integrations.YieldDetails;

namespace Closing.IntegrationEvents.Yields;

public sealed record GetYieldDetailsByPortfolioIdsAndClosingDateResponse(
    bool IsValid,
    IReadOnlyCollection<YieldDetailResponse> YieldDetails,
    string? Code,
    string? Message
);

