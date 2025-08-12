using Common.SharedKernel.Application.Messaging;
using Treasury.Domain.TreasuryMovements;

namespace Treasury.Integrations.TreasuryMovements.Queries;

public sealed record GetTreasuryMovementsByPortfolioIdsQuery(
    IEnumerable<long> PortfolioIds
    ): IQuery<IReadOnlyCollection<TreasuryMovement>>;
