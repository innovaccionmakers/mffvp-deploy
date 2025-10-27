using Common.SharedKernel.Domain;

namespace Closing.Application.Abstractions.External.Operations.TrustOperations;
public sealed record TrustYieldOperation(
    long TrustId,
    long OperationTypeId,
    decimal Amount,
    long? ClientOperationId,
    DateTime ProcessDateUtc
);

public sealed record UpsertTrustYieldOperationsBulkRemoteRequest(
    int PortfolioId,
    DateTime ClosingDateUtc,
    long OperationTypeId,
    IReadOnlyList<TrustYieldOperation> TrustYieldOperations,
    string? IdempotencyKey = null
);

public sealed record UpsertTrustYieldOperationsBulkRemoteResponse(
    int Inserted,
    int Updated,
    IReadOnlyCollection<long> ChangedTrustIds
);
/// <summary>
/// Command remoto al dominio Operations para upsert de operación de rendimientos.
/// </summary>
public interface IUpsertTrustYieldOperationsRemote
{
    Task<Result<UpsertTrustYieldOperationsBulkRemoteResponse>> UpsertYieldOperationsBulkAsync(
        UpsertTrustYieldOperationsBulkRemoteRequest request,
        CancellationToken cancellationToken);
}