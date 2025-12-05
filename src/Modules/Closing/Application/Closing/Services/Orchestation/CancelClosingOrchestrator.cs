using Closing.Application.Closing.Services.Abort;
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Application.Helpers.Time;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.Orchestation;

public class CancelClosingOrchestrator(
    IAbortClosingService abortClosing,
    ILogger<CancelClosingOrchestrator> logger)
    : ICancelClosingOrchestrator
{
    public async Task<Result<CancelClosingResult>> CancelAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        closingDate = DateTimeConverter.ToUtcDateTime(closingDate);
        logger.LogInformation("Cancelando cierre para portafolio {PortfolioId}", portfolioId);

        cancellationToken.ThrowIfCancellationRequested();

        var abortResult = await abortClosing.AbortAsync(portfolioId, closingDate, CancellationToken.None);

        if (abortResult.IsFailure)
        {
            logger.LogWarning(
                 "Falló el proceso de Cancelación para portafolio {PortfolioId}. Error: {ErrorCode} - {ErrorDescription}",
                 portfolioId,
                 abortResult.Error.Code,
                 abortResult.Error.Description);
            return Result.Failure<CancelClosingResult>(abortResult.Error);
        }

        logger.LogInformation("Cierre cancelado correctamente para portafolio {PortfolioId}", portfolioId);
        return Result.Success(new CancelClosingResult(portfolioId, closingDate, abortResult.IsSuccess));
    }
}
