namespace Closing.Application.PostClosing.Services.PortfolioUpdate;

public interface IPortfolioUpdateService
{
    Task ExecuteAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken);
}
