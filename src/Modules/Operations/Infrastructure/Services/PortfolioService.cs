using Products.Domain.Portfolios;
using Operations.Domain.Services;

namespace Operations.Infrastructure.Services;

public class PortfolioService : IPortfolioService
{
    private readonly IPortfolioRepository _portfolioRepository;

    public PortfolioService(IPortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    public async Task<DateTime> GetCurrentDateAsync(string portfolioId, CancellationToken cancellationToken = default)
    {
        var portfolio = await _portfolioRepository.GetAsync(int.Parse(portfolioId), cancellationToken);
        if (portfolio == null)
            throw new InvalidOperationException("Portfolio not found.");

        return portfolio.CurrentDate;
    }

    public async Task<DateTime> GetNextDateFromCurrentDateAsync(string portfolioId, CancellationToken cancellationToken = default)
    {
        var currentDate = await GetCurrentDateAsync(portfolioId, cancellationToken);

        return currentDate.AddDays(1);
    }
}