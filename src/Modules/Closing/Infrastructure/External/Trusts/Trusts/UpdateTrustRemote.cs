
using Closing.Application.Abstractions.External.Trusts.Trusts;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Trusts.IntegrationEvents.TrustYields;

namespace Closing.Infrastructure.External.Trusts.Trusts;

internal sealed class UpdateTrustRemote(
    IRpcClient rpcClient,
    ILogger<UpdateTrustRemote> logger)
    : IUpdateTrustRemote
{
    public async Task<Result<UpdateTrustFromYieldBulkRemoteResponse>> UpdateFromYieldAsync(
        UpdateTrustFromYieldBulkRemoteRequest request,
        CancellationToken cancellationToken)
    {
        // Mapeo Closing a Trusts 
        var rows = (request.TrustsToUpdate ?? Array.Empty<UpdateTrustFromYieldItem>())
            .Select(x => new ApplyYieldRowDto(
                TrustId: x.TrustId,
                YieldAmount: x.YieldAmount,
                YieldRetention: x.YieldRetention,
                ClosingBalance: x.ClosingBalance))
            .ToArray();


        var rpcRequest = new UpdateTrustFromYieldRequest(
            PortfolioId: request.PortfolioId,
            ClosingDate: request.ClosingDate,
            BatchIndex: request.BatchIndex,
            Rows: rows,
            IdempotencyKey: request.IdempotencyKey
        );

        try
        {
            var rpcResponse = await rpcClient.CallAsync<
                UpdateTrustFromYieldRequest,
                UpdateTrustFromYieldResponse>(rpcRequest, cancellationToken);

            if (rpcResponse is null || !rpcResponse.Succeeded)
            {
                var code = rpcResponse?.Code ?? "TRU-RPC-FAIL";
                var message = rpcResponse?.Message ?? "Fallo al actualizar fideicomisos";

                logger.LogWarning(
                    "{Class} - {Code} {Message}. Portafolio {PortfolioId}, Fecha {ClosingDate}, IdemKey {IdemKey}",
                    nameof(UpdateTrustRemote), code, message, request.PortfolioId, request.ClosingDate, request.IdempotencyKey);

                return Result.Failure<UpdateTrustFromYieldBulkRemoteResponse>(Error.Validation(code, message));
            }

            var response = new UpdateTrustFromYieldBulkRemoteResponse(
                BatchIndex: rpcResponse.BatchIndex,
                Updated: rpcResponse.Updated,
                MissingTrustIds: rpcResponse.MissingTrustIds,
                ValidationMismatchTrustIds: rpcResponse.ValidationMismatchTrustIds
            );

            return Result.Success(response);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning(
                "{Class} - Operación cancelada. Portafolio {PortfolioId}, Fecha {ClosingDate}, IdemKey {IdemKey}",
                nameof(UpdateTrustRemote), request.PortfolioId, request.ClosingDate, request.IdempotencyKey);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "{Class} - Excepción no controlada. Portafolio {PortfolioId}, Fecha {ClosingDate}, IdemKey {IdemKey}",
                nameof(UpdateTrustRemote), request.PortfolioId, request.ClosingDate, request.IdempotencyKey);

            return Result.Failure<UpdateTrustFromYieldBulkRemoteResponse>(
                new Error("TRU-RPC-UNHANDLED", "Error inesperado al invocar el RPC de actualización de fideicomisos.", ErrorType.Failure));
        }
    }
}
