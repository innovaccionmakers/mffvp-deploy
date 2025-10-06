
using Closing.Application.Abstractions.External.Operations.TrustOperations;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Operations.IntegrationEvents.TrustOperations;

namespace Closing.Infrastructure.External.Operations.TrustOperations;

/// <summary>
/// Implementación Infra del command remoto a Operations (Upsert de operación de rendimiento).
/// </summary>
internal sealed class UpsertTrustYieldOperationsRemote(
    IRpcClient rpcClient,
    ILogger<UpsertTrustYieldOperationsRemote> logger)
    : IUpsertTrustYieldOperationsRemote
{
    public async Task<Result<UpsertTrustYieldOperationRemoteResponse>> UpsertYieldOperationAsync(
        UpsertTrustYieldOperationRemoteRequest request,
        CancellationToken cancellationToken)
    {
        var rpcRequest = new CreateTrustYieldOperationRequest(
            TrustId: request.TrustId,
            PortfolioId: request.PortfolioId,
            ClosingDate: request.ClosingDate,
            OperationTypeId:request.OperationTypeId,
            YieldAmount: request.YieldAmount,
            YieldRetention: request.YieldRetention,
            ProcessDate: request.ProcessDate,
            ClosingBalance: request.ClosingBalance//,
            //CorrelationId: request.CorrelationId
        );

        var rpcResponse = await rpcClient.CallAsync<
            CreateTrustYieldOperationRequest,
            CreateTrustYieldOperationResponse>(rpcRequest, cancellationToken);

        if (rpcResponse is null || !rpcResponse.Succeeded)
        {
            var code = rpcResponse?.Code ?? "OPS-RPC-FAIL";
            var message = rpcResponse?.Message ?? "Fallo en Upsert de operación de rendimiento (sin respuesta).";
            logger.LogWarning("{Class} - {Code} {Message}", nameof(UpsertTrustYieldOperationsRemote), code, message);
            return Result.Failure<UpsertTrustYieldOperationRemoteResponse>(Error.Validation(code, message));
        }

        return Result.Success(new UpsertTrustYieldOperationRemoteResponse(rpcResponse.Succeeded, rpcResponse.Code, rpcResponse.Message, rpcResponse.OperationId));
    }
}