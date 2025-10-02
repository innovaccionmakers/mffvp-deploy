using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.Treasuries.GetAccountingConceptsTreasuries
{
    public sealed record class GetAccountingConceptsTreasuriesQuery(
        IEnumerable<int> PortfolioIds
        ) : IQuery<IReadOnlyCollection<GetAccountingConceptsTreasuriesResponse>>;
}
