using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.Treasuries.GetAccountingOperationsTreasuries
{
    public sealed record class GetAccountingOperationsTreasuriesQuery(
        IEnumerable<int> PortfolioIds,
        IEnumerable<string> CollectionAccount
        ) : IQuery<IReadOnlyCollection<GetAccountingOperationsTreasuriesResponse>>;
}
