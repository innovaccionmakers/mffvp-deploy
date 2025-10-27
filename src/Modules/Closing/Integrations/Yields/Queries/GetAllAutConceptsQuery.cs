using Common.SharedKernel.Application.Messaging;

namespace Closing.Integrations.Yields.Queries
{
    public sealed record class GetAllAutConceptsQuery(IEnumerable<int> PortfolioIds, DateTime ClosingDate) : IQuery<YieldAutConceptsCompleteResponse>;
}
