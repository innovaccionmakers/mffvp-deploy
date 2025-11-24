using Common.SharedKernel.Application.Messaging;

namespace Closing.Integrations.YieldDetails.Queries;

public sealed record GetYieldDetailsByPortfolioIdsAndClosingDateQuery(
    IEnumerable<int> PortfolioIds,
    DateTime ClosingDate,
    string Soruce
) : IQuery<IReadOnlyCollection<YieldDetailResponse>>;

