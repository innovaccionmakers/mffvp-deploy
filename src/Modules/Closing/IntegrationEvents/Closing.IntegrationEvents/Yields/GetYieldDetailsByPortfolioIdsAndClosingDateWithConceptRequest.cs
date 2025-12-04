namespace Closing.IntegrationEvents.Yields;

public sealed record GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptRequest(
    IEnumerable<int> PortfolioIds,
    DateTime ClosingDate,
    string Source,
    IEnumerable<Guid> GuidConcepts
);

