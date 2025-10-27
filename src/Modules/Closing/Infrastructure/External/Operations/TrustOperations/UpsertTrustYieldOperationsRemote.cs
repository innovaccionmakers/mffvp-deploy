using Closing.Application.Abstractions.External.Operations.TrustOperations;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Operations.IntegrationEvents.TrustOperations;
using Operations.Integrations.TrustOperations.Commands;

namespace Closing.Infrastructure.External.Operations.TrustOperations;

/// <summary>
/// Cliente hacia Operations para Upsert masivo de operaciones de rendimiento.
/// </summary>
internal sealed class UpsertTrustYieldOperationsRemote(
    IRpcClient rpcClient,
    ILogger<UpsertTrustYieldOperationsRemote> logger)
    : IUpsertTrustYieldOperationsRemote
{
    public async Task<Result<UpsertTrustYieldOperationsBulkRemoteResponse>> UpsertYieldOperationsBulkAsync(
        UpsertTrustYieldOperationsBulkRemoteRequest request,
        CancellationToken cancellationToken)
    {
        // Mapeo Closing.TrustYieldOperation a Operations.TrustYieldOperationFromClosing
        var opsItems = request.TrustYieldOperations.Select(i =>
            new TrustYieldOperationFromClosing(
                TrustId: i.TrustId,
                OperationTypeId: i.OperationTypeId,
                Amount: i.Amount,
                ClientOperationId: i.ClientOperationId,
                ProcessDateUtc: i.ProcessDateUtc
            )
        ).ToList();

        var rpcRequest = new CreateTrustYieldOpFromClosingRequest(
            PortfolioId: request.PortfolioId,
            ClosingDateUtc: request.ClosingDateUtc,
            OperationTypeId: request.OperationTypeId,
            TrustYieldOperations: opsItems,
            IdempotencyKey: request.IdempotencyKey
        );

        var rpcResponse = await rpcClient.CallAsync<
            CreateTrustYieldOpFromClosingRequest,
            CreateTrustYieldOpFromClosingResponse>(rpcRequest, cancellationToken);

        if (rpcResponse is null || !rpcResponse.Succeeded)
        {
            var code = rpcResponse?.Code ?? "OPS-RPC-FAIL";
            var message = rpcResponse?.Message ?? "Fallo en Upsert BULK de operaciones de rendimiento.";
            logger.LogWarning("{Class} - {Code} {Message}", nameof(UpsertTrustYieldOperationsRemote), code, message);

            return Result.Failure<UpsertTrustYieldOperationsBulkRemoteResponse>(
                Error.Validation(code, message));
        }

        var result = new UpsertTrustYieldOperationsBulkRemoteResponse(
            Inserted: rpcResponse.Inserted,
            Updated: rpcResponse.Updated,
            ChangedTrustIds: rpcResponse.ChangedTrustIds
        );

        return Result.Success(result);
    }
}
