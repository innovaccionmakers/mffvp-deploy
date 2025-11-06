using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.Treasuries.GetAccountingConceptsTreasuries
{
    public sealed record class GetAccountingConceptsTreasuriesQuery(
        IEnumerable<int> PortfolioIds,
        IEnumerable<string> AccountNumbers
        ) : IQuery<IReadOnlyCollection<GetAccountingConceptsTreasuriesResponse>>;
}
