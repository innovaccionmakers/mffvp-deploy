using Common.SharedKernel.Application.Messaging;
using Treasury.Integrations.TreasuryMovements.Response;

namespace Treasury.Integrations.TreasuryMovements.Queries;
public sealed record GetMovementsByPortfolioIdQuery(
    int PortfolioId,
    DateTime ClosingDate
    ) : IQuery<IReadOnlyCollection<GetMovementsByPortfolioIdResponse>>;
