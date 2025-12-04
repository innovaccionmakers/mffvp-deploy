using Common.SharedKernel.Application.Messaging;

namespace Closing.Integrations.YieldDetails.Queries;

public sealed record GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptQuery(
    IEnumerable<int> PortfolioIds,
    DateTime ClosingDate,
    string Source,
    IEnumerable<Guid> GuidConcepts
) : IQuery<IReadOnlyCollection<YieldDetailResponse>>;

