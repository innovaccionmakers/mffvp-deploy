using Closing.Domain.PortfolioValuations;

namespace Closing.Application.Closing.Services.Abort;
public sealed class AbortPortfolioValuationService(
      IPortfolioValuationRepository valuationRepository) : IAbortPortfolioValuationService
{
    public async Task DeleteClosedValuationAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {

        await valuationRepository.DeleteClosedByPortfolioAndDateAsync(portfolioId, closingDate, cancellationToken);
    }
}
