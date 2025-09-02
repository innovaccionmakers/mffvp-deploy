using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using DataSync.Application.TrustSync.Services.Interfaces;
using DataSync.Integrations.TrustSync;
using Microsoft.Extensions.Logging;

namespace DataSync.Application.TrustSync.Commands;

internal sealed class TrustSyncCommandHandler(
        ITrustSyncClosingService closingService,
        ILogger<TrustSyncCommandHandler> logger
    ) : ICommandHandler<TrustSyncCommand, bool>
{
    public async Task<Result<bool>> Handle(TrustSyncCommand command, CancellationToken cancellationToken)
    {
        if (command.PortfolioId <= 0)
            return Result.Failure<bool>(new Error(
                code: "DS-TS-001",
                description: "El identificador de portafolio debe ser un entero positivo.",
                type: ErrorType.Validation));
        try
        {
            var affected = await closingService.ExecuteAsync(command.PortfolioId, command.ClosingDate, cancellationToken);

            logger.LogInformation(
                "DataSync.TrustSync (Preclosing) completado. Portafolio={PortfolioId}, Fecha={ClosingDate}, FilasAfectadas={Affected}",
                command.PortfolioId, command.ClosingDate, affected);

            return Result.Success(true);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("DataSync.TrustSync cancelado por token. Portafolio={PortfolioId}, Fecha={ClosingDate}",
                command.PortfolioId, command.ClosingDate);
            return Result.Failure<bool>(new Error("DS-TS-008", "Operación cancelada.", ErrorType.Validation));
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex,
                "Error en DataSync.TrustSync. Portafolio={PortfolioId}, Fecha={ClosingDate}",
                command.PortfolioId, command.ClosingDate);

            return Result.Failure<bool>(new Error(
                code: "DS-TS-999",
                description: "Ocurrió un error al sincronizar datos de fideicomisos (Closing): " + ex.Message,
                type: ErrorType.Failure));
        }
    }
}
