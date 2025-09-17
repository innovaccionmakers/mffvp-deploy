namespace Closing.IntegrationEvents.Yields;

public sealed record GetYieldsByPortfolioIdsAndClosingDateRequest(
    List<int> PortfolioIds,
    DateTime ClosingDate
);
