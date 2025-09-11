using Microsoft.IdentityModel.Tokens;
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

    public async Task<DateTime> GetCurrentDateAsync(string portfolioId, int objetiveId, CancellationToken cancellationToken = default)
    {
        var homologateCode = !portfolioId.IsNullOrEmpty()
            ? portfolioId
            : (await _portfolioLocator.GetHomologateCodeByObjetiveIdAsync(objetiveId, cancellationToken)).Value;

        var portfolioRes = await _portfolioLocator.FindByHomologatedCodeAsync(homologateCode, cancellationToken);

        if (portfolioRes == null)
            throw new InvalidOperationException("Portfolio not found.");

        return portfolioRes.Value.CurrentDate;
    }

    public async Task<DateTime> GetNextDateFromCurrentDateAsync(string portfolioId, int objetiveId, CancellationToken cancellationToken = default)
    {
        var currentDate = await GetCurrentDateAsync(portfolioId, objetiveId, cancellationToken);

        return currentDate.AddDays(1);
    }
}