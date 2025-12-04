namespace Closing.IntegrationEvents.Yields;

public sealed record GetYieldDetailsByPortfolioIdsAndClosingDateRequest(
    IEnumerable<int> PortfolioIds,
    DateTime ClosingDate,
    string Source
);

