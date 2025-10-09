using Common.SharedKernel.Domain;
using MediatR;

namespace Operations.Integrations.TrustOperations.Commands;

public sealed record UpsertTrustOperationCommand(
    long TrustId,
    int PortfolioId,
    long OperationTypeId,
    decimal Amount,
    DateTime ClosingDate,
    DateTime ProcessDate,
    decimal YieldRetention,
    decimal ClosingBalance
) : IRequest<Result<UpsertTrustOperationResponse>>;