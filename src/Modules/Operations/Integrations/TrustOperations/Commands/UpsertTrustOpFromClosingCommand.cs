using Common.SharedKernel.Domain;
using MediatR;

namespace Operations.Integrations.TrustOperations.Commands;

public sealed record UpsertTrustOpFromClosingCommand(
    int PortfolioId,
    IReadOnlyList<TrustYieldOperationFromClosing> TrustYieldOperations
) : IRequest<Result<UpsertTrustOpFromClosingResponse>>;