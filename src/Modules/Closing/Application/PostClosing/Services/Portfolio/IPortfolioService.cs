namespace Closing.Application.PostClosing.Services.PortfolioServices;

public interface IPortfolioService
{
    Task UpdateAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken);

    Task<int> GetAsync(int portfolioId, CancellationToken cancellationToken);
}
