namespace Closing.IntegrationEvents.Yields;

public sealed record GetYieldsByPortfolioIdsAndClosingDateRequest(
    IEnumerable<int> PortfolioIds,
    DateTime ClosingDate
);
