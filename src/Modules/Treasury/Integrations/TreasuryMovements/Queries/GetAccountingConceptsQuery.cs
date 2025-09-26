using Common.SharedKernel.Application.Messaging;
using Treasury.Domain.TreasuryMovements;

namespace Treasury.Integrations.TreasuryMovements.Queries
{
    public sealed record class GetAccountingConceptsQuery(
        IEnumerable<int> PortfolioIds,
        DateTime ProcessDate
        ) : IQuery<IReadOnlyCollection<TreasuryMovement>>;
}
