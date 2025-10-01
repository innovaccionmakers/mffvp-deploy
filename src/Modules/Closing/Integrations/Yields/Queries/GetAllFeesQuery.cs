using Common.SharedKernel.Application.Messaging;

namespace Closing.Integrations.Yields.Queries;

public sealed record GetAllFeesQuery(IEnumerable<int> PortfolioIds, DateTime ClosingDate) : IQuery<IReadOnlyCollection<YieldResponse>>;
