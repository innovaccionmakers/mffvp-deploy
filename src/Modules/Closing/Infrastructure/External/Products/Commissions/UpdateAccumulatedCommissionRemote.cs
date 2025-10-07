

using Closing.Application.Abstractions.External.Products.AccumulatedCommissions;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Products.IntegrationEvents.AccumulatedCommissions;

namespace Closing.Infrastructure.External.Products.Commissions;

internal sealed class UpdateAccumulatedCommissionRemote(
    IRpcClient rpcClient,
    ILogger<UpdateAccumulatedCommissionRemote> logger)
    : IUpdateAccumulatedCommissionRemote
{

    public async Task<Result<UpdateAccumulatedCommissionRemoteResponse>> ExecuteAsync(
        UpdateAccumulatedCommissionRemoteRequest request,
        CancellationToken cancellationToken)
    {
     
        // Mapear a los DTOs que espera Products
        var rpcRequest = new UpdateAccumulatedCommissionFromClosingRequest(
            PortfolioId: request.PortfolioId,
            CommissionId: request.CommissionId,
            AccumulatedValue: request.AccumulatedValue,
            ClosingDate: request.ClosingDateUtc,
            IdempotencyKey: request.IdempotencyKey,
            Origin: request.Origin,
            ExecutionId: request.ExecutionId
        );

        var rpcResponse = await rpcClient.CallAsync<
            UpdateAccumulatedCommissionFromClosingRequest,
            UpdateAccumulatedCommissionFromClosingResponse>(rpcRequest, cancellationToken);

        if (rpcResponse is null || !rpcResponse.Succeeded)
        {
            var code = rpcResponse?.Code ?? "PROD-RPC-FAIL";
            var message = rpcResponse?.Message ?? "Fallo en actualización de comisión acumulada (sin respuesta).";
            logger.LogWarning("{Class} - {Code} {Message}", nameof(UpdateAccumulatedCommissionRemote), code, message);
            return Result.Failure<UpdateAccumulatedCommissionRemoteResponse>(Error.Validation(code, message));
        }

        var response = new UpdateAccumulatedCommissionRemoteResponse(
            Succeeded: rpcResponse.Succeeded,
            Status: rpcResponse.Status ?? "Updated",
            Message: rpcResponse.Message
        );

        return Result.Success(response);
    }
}