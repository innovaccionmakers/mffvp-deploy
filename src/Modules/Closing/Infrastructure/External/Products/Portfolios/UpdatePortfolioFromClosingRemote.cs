using Closing.Application.Abstractions.External.Products.Portfolios;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Products.IntegrationEvents.Portfolio.UpdateFromClosing;

namespace Closing.Infrastructure.External.Products.Portfolios;

internal sealed class UpdatePortfolioFromClosingRemote(
    IRpcClient rpcClient,
    ILogger<UpdatePortfolioFromClosingRemote> logger)
    : IUpdatePortfolioFromClosingRemote
{
    private const string RpcMethod = "Product.Portfolios.UpdateFromClosing";

    public async Task<Result<UpdatePortfolioFromClosingRemoteResponse>> ExecuteAsync(
        UpdatePortfolioFromClosingRemoteRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("[{Class}] {Method} → Portfolio={PortfolioId} Date={Date} IdemKey={Key} Origin={Origin}",
            nameof(UpdatePortfolioFromClosingRemote), RpcMethod,
            request.PortfolioId, request.ClosingDateUtc, request.IdempotencyKey, request.Origin);

        var rpcRequest = new UpdatePortfolioFromClosingRequest(
            PortfolioId: request.PortfolioId,
            ClosingDate: request.ClosingDateUtc,
            IdempotencyKey: request.IdempotencyKey,
            Origin: request.Origin,                 // <-- mapeo del origen
            ExecutionId: request.ExecutionId
        );

        var rpcResponse = await rpcClient.CallAsync<
            UpdatePortfolioFromClosingRequest,
            UpdatePortfolioFromClosingResponse>(rpcRequest, cancellationToken);

        if (rpcResponse is null || !rpcResponse.Succeeded)
        {
            var code = rpcResponse?.Code ?? "PROD-RPC-FAIL";
            var message = rpcResponse?.Message ?? "Fallo en actualización de portafolio (sin respuesta).";
            logger.LogWarning("{Class} - {Code} {Message}", nameof(UpdatePortfolioFromClosingRemote), code, message);
            return Result.Failure<UpdatePortfolioFromClosingRemoteResponse>(Error.Validation(code, message));
        }

        return Result.Success(new UpdatePortfolioFromClosingRemoteResponse(
            rpcResponse.Succeeded, rpcResponse.Status ?? "Updated",
            rpcResponse.UpdatedCount, rpcResponse.Message));
    }
}
