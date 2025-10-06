
using Closing.Application.Abstractions.External.Trusts.Trusts;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Trusts.IntegrationEvents.TrustYields;

namespace Closing.Infrastructure.External.Trusts.Trusts;


/// <summary>
/// Implementación Infra del command remoto a Trusts (aplicar rendimiento al saldo).
/// </summary>
internal sealed class UpdateTrustRemote(
    IRpcClient rpcClient,
    ILogger<UpdateTrustRemote> logger)
    : IUpdateTrustRemote
{
    public async Task<Result<UpdateTrustFromYieldRemoteResponse>> UpdateFromYieldAsync(
        UpdateTrustFromYieldRemoteRequest request,
        CancellationToken cancellationToken)
    {
        var rpcRequest = new UpdateTrustFromYieldRequest(
            TrustId: request.TrustId,
            YieldAmount: request.YieldAmount,
            YieldRetention: request.YieldRetention,
            ClosingBalance: request.ClosingBalance,
            PortfolioId: request.PortfolioId,
            ClosingDate: request.ClosingDate
            //ValidateConsistency: request.ValidateConsistency,
            //Strict: request.Strict,
            //Tolerance: request.Tolerance,
            //CorrelationId: request.CorrelationId
        );

        var rpcResponse = await rpcClient.CallAsync<
            UpdateTrustFromYieldRequest,
            UpdateTrustFromYieldResponse>(rpcRequest, cancellationToken);

        if (rpcResponse is null || !rpcResponse.Succeeded)
        {
            var code = rpcResponse?.Code ?? "TRU-RPC-FAIL";
            var message = rpcResponse?.Message ?? "Fallo al actualizar fideicomiso (sin respuesta).";
            logger.LogWarning("{Class} - {Code} {Message}", nameof(UpdateTrustRemote), code, message);
            return Result.Failure<UpdateTrustFromYieldRemoteResponse>(Error.Validation(code, message));
        }

        return Result.Success(new UpdateTrustFromYieldRemoteResponse());
    }
}