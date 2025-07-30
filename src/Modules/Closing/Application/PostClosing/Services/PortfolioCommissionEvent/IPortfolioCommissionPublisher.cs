namespace Closing.Application.PostClosing.Services.PortfolioCommissionEvent;

public interface IPortfolioCommissionPublisher
{
    Task PublishAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken);
}
