using Closing.Integrations.Yields;

namespace Closing.IntegrationEvents.Yields;

public sealed record GetAllComissionsByPortfolioIdsAndClosingDateResponse(
    bool IsValid,
    IReadOnlyCollection<YieldResponse> Yields,
    string? Code,
    string? Message
);
