

namespace Closing.Application.PostClosing.Services.PortfolioValuationEvent;

public interface IPortfolioValuationPublisher
{
    Task PublishAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken);
}
