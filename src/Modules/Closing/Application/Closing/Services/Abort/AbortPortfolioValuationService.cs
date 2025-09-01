
using Closing.Domain.PortfolioValuations;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.Abort;
public sealed class AbortPortfolioValuationService(
      IPortfolioValuationRepository valuationRepository,
      ILogger<AbortPortfolioValuationService> logger) : IAbortPortfolioValuationService
{
    public async Task DeleteClosedValuationAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        logger.LogInformation("Eliminando Portfolio Valuation cerrado para Portafolio {PortfolioId} - Fecha {Date}", portfolioId, closingDate);

        await valuationRepository.DeleteClosedByPortfolioAndDateAsync(portfolioId, closingDate, cancellationToken);
    }
}
