
using Closing.Domain.YieldDetails;
using Closing.Domain.Yields;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.Abort;

public sealed class AbortSimulationService(
      IYieldDetailRepository yieldDetailRepository,
      IYieldRepository yieldRepository) : IAbortSimulationService
{
    public async Task DeleteClosedSimulationAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        await yieldDetailRepository.DeleteClosedByPortfolioAndDateAsync(portfolioId, closingDate, cancellationToken);
        await yieldRepository.DeleteClosedByPortfolioAndDateAsync(portfolioId, closingDate, cancellationToken);
    }
}