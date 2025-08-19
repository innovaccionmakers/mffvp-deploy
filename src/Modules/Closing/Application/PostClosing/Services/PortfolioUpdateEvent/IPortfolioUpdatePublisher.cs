namespace Closing.Application.PostClosing.Services.PortfolioUpdateEvent;

public interface IPortfolioUpdatePublisher
{
    Task PublishAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken);
}
