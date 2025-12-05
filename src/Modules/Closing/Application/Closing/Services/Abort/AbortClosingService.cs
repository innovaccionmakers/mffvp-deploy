using Closing.Application.Abstractions.Data;
using Closing.Application.Closing.Services.Telemetry;
using Closing.Application.PostClosing.Services.PendingTransactions;
using Common.SharedKernel.Application.Caching.Closing.Interfaces;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.Abort;
public sealed class AbortClosingService(
      IClosingExecutionStore store,
      IAbortSimulationService abortSimulationService,
      IAbortPortfolioValuationService abortPortfolioValuationService,
      IAbortTrustYieldService abortTrustYieldService,
      IPendingTransactionsService pendingTransactionHandler,
      IUnitOfWork unitOfWork,
      IClosingStepTimer stepTimer,
      ILogger<AbortClosingService> logger) : IAbortClosingService
{
    public async Task<Result> AbortAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {

        if (cancellationToken.IsCancellationRequested)
        {
            cancellationToken.ThrowIfCancellationRequested();
        }

        // Token para ejecutar el abort completo sin riesgo de cortarlo a medias.
        var abortToken = CancellationToken.None;

        var isActive = await store.IsClosingActiveAsync(abortToken);
        if (!isActive)
        {
            return Result.Failure(new Error("001", "No hay cierre activo para el portafolio.", ErrorType.Validation));
        }

        var closingDateUtc = DateTime.SpecifyKind(closingDate.Date, DateTimeKind.Utc);

        await using var transaction = await unitOfWork.BeginTransactionAsync(abortToken);

        try
        {
            // 1) Borrar simulación cerrada
            using (stepTimer.Track("Abort.DeleteSimulation", portfolioId, closingDateUtc))
            {
                await abortSimulationService.DeleteClosedSimulationAsync(portfolioId, closingDateUtc, abortToken);
            }

            // 2) Borrar valoración cerrada
            using (stepTimer.Track("Abort.DeleteValuation", portfolioId, closingDateUtc))
            {
                await abortPortfolioValuationService.DeleteClosedValuationAsync(portfolioId, closingDateUtc, abortToken);
            }

            // 3) Borrar rendimientos de fideicomisos (set-based) y registrar conteo
            using (stepTimer.Track("Abort.DeleteTrustYields", portfolioId, closingDateUtc))
            {
                var deleted = await abortTrustYieldService.DeleteTrustYieldsAsync(portfolioId, closingDateUtc, abortToken);
                stepTimer.SetResultCount(deleted); 
            }

            // 4) Guardar cambios
            using (stepTimer.Track("Abort.UnitOfWork.SaveChanges", portfolioId, closingDateUtc))
            {
                await unitOfWork.SaveChangesAsync(abortToken);
            }

            // 5) Commit transacción
            using (stepTimer.Track("Abort.Transaction.Commit", portfolioId, closingDateUtc))
            {
                await transaction.CommitAsync(abortToken);
            }

            // 6) Reactivar flujo (pendientes)
            using (stepTimer.Track("Abort.PendingTransactions", portfolioId, closingDateUtc))
            {
                await pendingTransactionHandler.HandleAsync(portfolioId, closingDateUtc, abortToken);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(abortToken);
            logger.LogError(ex, "AbortClosingService | Error abortando cierre. Portafolio={PortfolioId}, Fecha={ClosingDateUtc:O}",
                portfolioId, closingDateUtc);
            return Result.Failure(new Error("002", "Error al abortar el cierre.", ErrorType.Failure));
        }
    }
}