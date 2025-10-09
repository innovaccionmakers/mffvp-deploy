using Common.SharedKernel.Domain;

namespace Closing.Application.Abstractions.External.Operations.TrustOperations;
public sealed record UpsertTrustYieldOperationRemoteRequest(
    long TrustId,
    int PortfolioId,
    DateTime ClosingDate,
    long OperationTypeId,
    decimal YieldAmount,
    decimal YieldRetention,
    DateTime ProcessDate,
    decimal ClosingBalance,
    string? IdempotencyKey = null
);

public sealed record UpsertTrustYieldOperationRemoteResponse(
       bool Succeeded,
       string? Code,
       string? Message,
       long? OperationId = null
   );

/// <summary>
/// Command remoto al dominio Operations para upsert de operación de rendimientos.
/// </summary>
public interface IUpsertTrustYieldOperationsRemote
{
    Task<Result<UpsertTrustYieldOperationRemoteResponse>> UpsertYieldOperationAsync(
        UpsertTrustYieldOperationRemoteRequest request,
        CancellationToken cancellationToken);
}