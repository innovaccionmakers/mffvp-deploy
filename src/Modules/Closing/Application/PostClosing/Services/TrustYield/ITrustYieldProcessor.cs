

namespace Closing.Application.PostClosing.Services.TrustYield;

public interface ITrustYieldProcessor
{
    Task ProcessAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken);
}