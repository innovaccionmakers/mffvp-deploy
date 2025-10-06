
using Closing.Application.Abstractions.External.Operations.ClientOperations;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Operations.IntegrationEvents.PendingContributionProcessor;

namespace Closing.Infrastructure.External.Operations.ClientOperations;
internal sealed class ProcessPendingTransactionsRemote(
    IRpcClient rpcClient,
    ILogger<ProcessPendingTransactionsRemote> logger)
    : IProcessPendingTransactionsRemote
{
    public async Task<Result<ProcessPendingTransactionsRemoteResponse>> ExecuteAsync(
        ProcessPendingTransactionsRemoteRequest request,
        CancellationToken cancellationToken)
    {
        // Mapea a los DTOs esperados por el servicio de Operations
        var rpcRequest = new ProcessPendingTransactionsRequest(
            PortfolioId: request.PortfolioId,
            ProcessDate: request.ProcessDateUtc,
            IdempotencyKey: request.IdempotencyKey,
            ExecutionId: request.ExecutionId
        );

        var rpcResponse = await rpcClient.CallAsync<
            ProcessPendingTransactionsRequest,
            ProcessPendingTransactionsResponse>(rpcRequest, cancellationToken);

        if (rpcResponse is null || !rpcResponse.Succeeded)
        {
            var code = rpcResponse?.Code ?? "OPS-RPC-FAIL";
            var message = rpcResponse?.Message ?? "Fallo en procesamiento de transacciones pendientes (sin respuesta).";
            logger.LogWarning("{Class} - {Code} {Message}", nameof(ProcessPendingTransactionsRemote), code, message);
            return Result.Failure<ProcessPendingTransactionsRemoteResponse>(Error.Validation(code, message));
        }

        // Éxito: empaqueta en tu Response
        var response = new ProcessPendingTransactionsRemoteResponse(
            Succeeded: rpcResponse.Succeeded,
            Status: rpcResponse.Status ?? "Processed",
            AppliedCount: rpcResponse.AppliedCount,
            SkippedCount: rpcResponse.SkippedCount,
            Message: rpcResponse.Message
        );

        return Result.Success(response);
    }
}