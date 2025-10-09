namespace Closing.Application.PostClosing.Services.PortfolioCommission;

public interface IPortfolioCommissionService
{
    Task ExecuteAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken);
}
