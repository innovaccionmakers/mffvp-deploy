using Closing.Application.Abstractions.External.Products.Portfolios;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Products.IntegrationEvents.Portfolio.GetInfoForClosing;

namespace Closing.Infrastructure.External.Products.Portfolios;

internal sealed class GetPortfolioDataRemote(IRpcClient rpcClient)
      : IGetPortfolioDataRemote
{
    public async Task<Result<GetPortfolioClosingDataRemoteResponse>> GetAsync(
      GetPortfolioClosingDataRemoteRequest request,
      CancellationToken cancellationToken)
    {


        var rpcRequest = new GetPortfolioInfoForClosingRequest(
            PortfolioId: request.PortfolioId    
        );

        var rpcResponse = await rpcClient.CallAsync<
            GetPortfolioInfoForClosingRequest,
            GetPortfolioInfoForClosingResponse>(rpcRequest, cancellationToken);

        if (rpcResponse is null || !rpcResponse.Succeeded)
        {
            var code = rpcResponse?.Code ?? "PROD-RPC-FAIL";
            var message = rpcResponse?.Message ?? "Fallo en obtencion de datos de portafolio (sin respuesta).";

            return Result.Failure<GetPortfolioClosingDataRemoteResponse>(Error.Validation(code, message));
        }

        return Result.Success(new GetPortfolioClosingDataRemoteResponse(
            rpcResponse.Succeeded, rpcResponse.AgileWithdrawalPercentageProtectedBalance,
            rpcResponse.Code, rpcResponse.Message));
    }
}
