using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.Treasuries.GetTreasuriesByPortfolioIds
{
    public sealed record class GetTreasuriesByPortfolioIdsQuery(
        IEnumerable<int> PortfolioIds
        ) : IQuery<IReadOnlyCollection<GetTreasuriesByPortfolioIdsResponse>>;
}
