namespace Closing.IntegrationEvents.YieldsToDistribute;

public sealed record GetDistributedYieldGroupByConceptRequest
(
    IEnumerable<int> PortfolioIds,
    DateTime ClosingDate,
    string Concept
);
