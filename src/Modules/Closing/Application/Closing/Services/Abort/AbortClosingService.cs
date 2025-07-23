using Closing.Application.Abstractions.Data;
using Closing.Application.ClosingWorkflow;
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
    IClosingWorkflowService workflowService,
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
            await yieldDetailRepository.DeleteClosedByPortfolioAndDateAsync(portfolioId, closingDate, ct);
            await yieldRepository.DeleteClosedByPortfolioAndDateAsync(portfolioId, closingDate, ct);
            await valuationRepository.DeleteClosedByPortfolioAndDateAsync(portfolioId, closingDate, ct);

            await unitOfWork.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            logger.LogError(ex, "Error abortando cierre para Portafolio {PortfolioId} - Fecha {Date}", portfolioId, closingDate);
            return Result.Failure(new Error("002", "Error al abortar el cierre.", ErrorType.Failure));
        }

        await workflowService.EndAsync(portfolioId, ct);
        return Result.Success();
    }
}