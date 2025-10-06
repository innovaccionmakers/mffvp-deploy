namespace Closing.IntegrationEvents.Yields;

public sealed record GetAllComissionsByPortfolioIdsAndClosingDateRequest(
    IEnumerable<int> PortfolioIds,
    DateTime ClosingDate
);
