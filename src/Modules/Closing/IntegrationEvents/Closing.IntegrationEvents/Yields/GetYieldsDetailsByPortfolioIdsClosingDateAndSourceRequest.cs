namespace Closing.IntegrationEvents.Yields;

public sealed record GetYieldsDetailsByPortfolioIdsClosingDateAndSourceRequest(
    IEnumerable<int> PortfolioIds,
    DateTime ClosingDate,
    string Source,
    string? Concept
);

