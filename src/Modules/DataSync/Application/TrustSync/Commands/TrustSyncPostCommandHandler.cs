
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using DataSync.Application.TrustSync.Services.Interfaces;
using DataSync.Integrations.TrustSync;
using Microsoft.Extensions.Logging;

namespace DataSync.Application.TrustSync.Commands;

internal sealed class TrustSyncPostCommandHandler(
       ITrustSyncPostService postService,
       ILogger<TrustSyncPostCommandHandler> logger
   ) : ICommandHandler<TrustSyncPostCommand, bool>
{
    public async Task<Result<bool>> Handle(TrustSyncPostCommand command, CancellationToken cancellationToken)
    {
        if (command.PortfolioId <= 0)
            return Result.Failure<bool>(new Error(
                code: "DS-TSP-001",
                description: "El identificador de portafolio debe ser un entero positivo.",
                type: ErrorType.Validation));

        var closingDate = command.ClosingDate.Date;

        try
        {
            // Orquestación post-cierre:
            // Lee (fideicomiso, unidades) desde cierre.rendimientos_fideicomisos
            // y actualiza trust.fideicomisos.unidades
            var updated = await postService.ExecuteAsync(command.PortfolioId, closingDate, cancellationToken);

            logger.LogInformation(
                "DataSync.TrustSyncPost completado. Portafolio={PortfolioId}, Fecha={ClosingDate}, FilasActualizadas={Updated}",
                command.PortfolioId, closingDate, updated);

            return Result.Success(true);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning(
                "DataSync.TrustSyncPost cancelado. Portafolio={PortfolioId}, Fecha={ClosingDate}",
                command.PortfolioId, closingDate);

            return Result.Failure<bool>(new Error("DS-TSP-008", "Operación cancelada.", ErrorType.Failure));
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error en DataSync.TrustSyncPost. Portafolio={PortfolioId}, Fecha={ClosingDate}",
                command.PortfolioId, closingDate);

            return Result.Failure<bool>(new Error(
                code: "DS-TSP-999",
                description: "Ocurrió un error al sincronizar unidades de fideicomisos (Post-closing): " + ex.Message,
                type: ErrorType.Failure));
        }
    }
}