using Common.SharedKernel.Application.Messaging;

namespace Closing.Integrations.Yields.Queries;

public sealed record GetAllReturnsQuery(IEnumerable<int> PortfolioIds, DateTime ClosingDate) : IQuery<IReadOnlyCollection<YieldResponse>>;
