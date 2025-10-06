using Accounting.Integrations.Concept.GetConceptsByPortfolioIds;
using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.Treasuries.GetConceptsByPortfolioIds
{
    public sealed record class GetConceptsByPortfolioIdsQuery(
        IEnumerable<int> PortfolioIds
        ) : IQuery<IReadOnlyCollection<GetConceptsByPortfolioIdsResponse>>;
}
