
using Closing.Domain.YieldDetails;
using Closing.Domain.Yields;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.Abort;

public sealed class AbortSimulationService(
      IYieldDetailRepository yieldDetailRepository,
      IYieldRepository yieldRepository,
      ILogger<AbortSimulationService> logger) : IAbortSimulationService
{
    public async Task DeleteClosedSimulationAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        logger.LogInformation("Eliminando YieldDetails y Yields cerrados para Portafolio {PortfolioId} - Fecha {Date}", portfolioId, closingDate);

        await yieldDetailRepository.DeleteClosedByPortfolioAndDateAsync(portfolioId, closingDate, cancellationToken);
        await yieldRepository.DeleteClosedByPortfolioAndDateAsync(portfolioId, closingDate, cancellationToken);
    }
}