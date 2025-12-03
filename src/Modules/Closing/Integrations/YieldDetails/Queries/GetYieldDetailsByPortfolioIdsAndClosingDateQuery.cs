using Common.SharedKernel.Application.Messaging;

namespace Closing.Integrations.YieldDetails.Queries;

public sealed record GetYieldDetailsByPortfolioIdsAndClosingDateQuery(
    IEnumerable<int> PortfolioIds,
    DateTime ClosingDate,
    string Source,
    Guid? GuidConcept
) : IQuery<IReadOnlyCollection<YieldDetailResponse>>;

