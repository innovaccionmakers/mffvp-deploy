namespace Closing.IntegrationEvents.Yields;

public sealed record GetAllReturnsByPortfolioIdsAndClosingDateRequest(
    IEnumerable<int> PortfolioIds,
    DateTime ClosingDate
);
