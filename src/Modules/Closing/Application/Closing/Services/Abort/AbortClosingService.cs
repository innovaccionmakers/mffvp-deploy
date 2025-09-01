using Closing.Application.Abstractions.Data;
using Closing.Application.PostClosing.Services.PendingTransactionHandler;
using Common.SharedKernel.Application.Caching.Closing.Interfaces;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.Abort;
public sealed class AbortClosingService(
      IClosingExecutionStore store,
      IAbortSimulationService abortSimulationService,
      IAbortPortfolioValuationService abortPortfolioValuationService,
      IPendingTransactionHandler pendingTransactionHandler,
      IUnitOfWork unitOfWork,
      ILogger<AbortClosingService> logger) : IAbortClosingService
{
    public async Task<Result> AbortAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        var isActive = await store.IsClosingActiveAsync(cancellationToken);
        if (!isActive)
        {
            return Result.Failure(new Error("001", "No hay cierre activo para el portafolio.", ErrorType.Validation));
        }

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            logger.LogInformation("Eliminando datos del cierre simulado para el portafolio {PortfolioId}", portfolioId);

            await abortSimulationService.DeleteClosedSimulationAsync(portfolioId, closingDate, cancellationToken);
            await abortPortfolioValuationService.DeleteClosedValuationAsync(portfolioId, closingDate, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("Datos eliminados correctamente. Reactivando flujo para el portafolio {PortfolioId}", portfolioId);

            await pendingTransactionHandler.HandleAsync(portfolioId, closingDate, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Error abortando cierre para Portafolio {PortfolioId} - Fecha {Date}", portfolioId, closingDate);
            return Result.Failure(new Error("002", "Error al abortar el cierre.", ErrorType.Failure));
        }
    }
}
