using Closing.Application.Abstractions.Data;
using Closing.Application.PostClosing.Services.PendingTransactionHandler;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.YieldDetails;
using Closing.Domain.Yields;
using Common.SharedKernel.Application.Caching.Closing.Interfaces;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.Abort;
public sealed class AbortClosingService(
    IClosingExecutionStore store,
    IYieldDetailRepository yieldDetailRepository,
    IYieldRepository yieldRepository,
    IPortfolioValuationRepository valuationRepository,
    IPendingTransactionHandler pendingTransactionHandler,
    IUnitOfWork unitOfWork,
    ILogger<AbortClosingService> logger) : IAbortClosingService
{
    public async Task<Result> AbortAsync(int portfolioId, DateTime closingDate, CancellationToken ct)
    {

        var isActive = await store.IsClosingActiveAsync(portfolioId, ct);
        if (!isActive)
        {
            return Result.Failure(new Error("001", "No hay cierre activo para el portafolio.", ErrorType.Validation));
        }

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            logger.LogInformation("Eliminando datos del cierre simulado para el portafolio {PortfolioId}", portfolioId);

            await yieldDetailRepository.DeleteClosedByPortfolioAndDateAsync(portfolioId, closingDate, ct);
            await yieldRepository.DeleteClosedByPortfolioAndDateAsync(portfolioId, closingDate, ct);
            await valuationRepository.DeleteClosedByPortfolioAndDateAsync(portfolioId, closingDate, ct);

            await unitOfWork.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            logger.LogInformation("Datos eliminados correctamente. Reactivando flujo para el portafolio {PortfolioId}", portfolioId);

            await pendingTransactionHandler.HandleAsync(portfolioId, closingDate, ct);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            logger.LogError(ex, "Error abortando cierre para Portafolio {PortfolioId} - Fecha {Date}", portfolioId, closingDate);
            return Result.Failure(new Error("002", "Error al abortar el cierre.", ErrorType.Failure));
        }
    }
}