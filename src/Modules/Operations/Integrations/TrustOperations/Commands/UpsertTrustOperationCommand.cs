using MediatR;

namespace Operations.Integrations.TrustOperations.Commands;

public sealed record UpsertTrustOperationCommand(
    long TrustId,
    int PortfolioId,
    decimal Amount,
    DateTime ClosingDate,
    DateTime ProcessDate,
    decimal YieldRetention,
    decimal ClosingBalance,
    decimal Units
) : IRequest;