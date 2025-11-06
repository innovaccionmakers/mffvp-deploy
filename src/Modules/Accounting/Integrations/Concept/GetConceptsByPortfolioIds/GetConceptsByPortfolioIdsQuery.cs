using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.Concept.GetConceptsByPortfolioIds;

public sealed record class GetConceptsByPortfolioIdsQuery(
    IEnumerable<int> PortfolioIds,
    IEnumerable<string> Concepts
    ) : IQuery<IReadOnlyCollection<GetConceptsByPortfolioIdsResponse>>;
