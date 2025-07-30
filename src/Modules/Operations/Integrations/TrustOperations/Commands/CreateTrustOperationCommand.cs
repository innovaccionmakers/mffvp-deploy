using MediatR;

namespace Operations.Integrations.TrustOperations.Commands;

public sealed record CreateTrustOperationCommand(
    long TrustId,
    int PortfolioId,
    decimal Amount,
    DateTime ClosingDate,
    DateTime ProcessDate
) : IRequest;