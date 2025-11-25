using Common.SharedKernel.Application.Messaging;

namespace Closing.Integrations.YieldsToDistribute.Queries;

public sealed record GetDistributedYieldsGroupedQuery
(
    IEnumerable<int> PortfolioIds,
    DateTime ClosingDate,
    string Concept
) : IQuery<IReadOnlyCollection<DistributedYieldGroupResponse>>;
