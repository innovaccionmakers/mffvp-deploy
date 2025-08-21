using Common.SharedKernel.Application.Rpc;
using DataSync.Integrations.TrustSync;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DataSync.IntegrationEvents.TrustSync;

public sealed class TrustSyncPostConsumer(
    IMediator mediator,
    ILogger<TrustSyncPostConsumer> logger
) : IRpcHandler<TrustSyncPostRequest, TrustSyncPostResponse>
{
    public async Task<TrustSyncPostResponse> HandleAsync(TrustSyncPostRequest request, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new TrustSyncPostCommand(request.ClosingDate, request.PortfolioId), ct);

            if (result.IsFailure)
            {
                logger.LogWarning("TrustSyncPost falló. Portafolio={PortfolioId} Fecha={Date} Code={Code} Msg={Msg}",
                    request.PortfolioId, request.ClosingDate, result.Error.Code, result.Error.Description);

                return new TrustSyncPostResponse(false, result.Error.Code, result.Error.Description);
            }

            logger.LogInformation("TrustSyncPost OK. Portafolio={PortfolioId} Fecha={Date}",
                request.PortfolioId, request.ClosingDate);

            return new TrustSyncPostResponse(true);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("TrustSyncPost cancelado por token. Portafolio={PortfolioId} Fecha={Date}",
                request.PortfolioId, request.ClosingDate);

            return new TrustSyncPostResponse(false, "DS-TSP-008", "Operación cancelada.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado en TrustSyncPost. Portafolio={PortfolioId} Fecha={Date}",
                request.PortfolioId, request.ClosingDate);

            return new TrustSyncPostResponse(false, "DS-TSP-999",
                "Error al sincronizar unidades de fideicomisos (Post-closing).");
        }
    }
}