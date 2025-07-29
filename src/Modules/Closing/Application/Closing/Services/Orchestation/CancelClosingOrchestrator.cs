using Closing.Application.Closing.Services.Abort;
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.Orchestation;

public class CancelClosingOrchestrator(
    IAbortClosingService abortClosing,
    ILogger<CancelClosingOrchestrator> logger)
    : ICancelClosingOrchestrator
{
    public async Task<Result<ClosedResult>> CancelAsync(int portfolioId, DateTime closingDate, CancellationToken ct)
    {
        logger.LogInformation("Cancelando cierre para portafolio {PortfolioId}", portfolioId);

        var abortResult = await abortClosing.AbortAsync(portfolioId, closingDate, ct);
        if (abortResult.IsFailure)
        {
            logger.LogWarning("Falló el proceso de Cancelación para portafolio {PortfolioId}", portfolioId);
            return Result.Failure<ClosedResult>(abortResult.Error);
        }

        logger.LogInformation("Cierre cancelado correctamente para portafolio {PortfolioId}", portfolioId);
        return Result.Success(new ClosedResult(portfolioId, closingDate));
    }
}
