using Operations.Application.Abstractions.External;
using Operations.Application.Abstractions.Services.Portfolio;

namespace Operations.Application.Portfolio.Services;

public class PortfolioService : IPortfolioService
{
    private readonly IPortfolioLocator _portfolioLocator;

    public PortfolioService(IPortfolioLocator portfolioLocator)
    {
        _portfolioLocator = portfolioLocator;
    }

    public async Task<DateTime> GetCurrentDateAsync(string portfolioId, CancellationToken cancellationToken = default)
    {
        var portfolioRes = await _portfolioLocator.FindByHomologatedCodeAsync(portfolioId, cancellationToken);
        if (portfolioRes == null)
            throw new InvalidOperationException("Portfolio not found.");

        return portfolioRes.Value.CurrentDate;
    }

    public async Task<DateTime> GetNextDateFromCurrentDateAsync(string portfolioId, CancellationToken cancellationToken = default)
    {
        var currentDate = await GetCurrentDateAsync(portfolioId, cancellationToken);

        return currentDate.AddDays(1);
    }
}