using Common.SharedKernel.Domain;

namespace Closing.Application.Abstractions.External.Operations.TrustOperations;

public interface ITrustYieldOperationsUpsertRemote
{
    Task<Result<CreateYieldOperationRemoteResponse>> CreateYieldOperationAsync(
        CreateYieldOperationRemoteRequest request,
        CancellationToken cancellationToken);
}

public sealed record CreateYieldOperationRemoteRequest(
    long TrustId,
    int PortfolioId,
    DateTime ClosingDate,
    decimal YieldAmount,
    decimal YieldRetention,
    DateTime ProcessDate,
    decimal ClosingBalance,
    string? CorrelationId = null
);

public sealed record CreateYieldOperationRemoteResponse(
    long? OperationId
);