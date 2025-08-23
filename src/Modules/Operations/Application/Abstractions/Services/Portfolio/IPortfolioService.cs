namespace Operations.Application.Abstractions.Services.Portfolio;

public interface IPortfolioService
{
    Task<DateTime> GetCurrentDateAsync(string portfolioId, CancellationToken cancellationToken = default);

    Task<DateTime> GetNextDateFromCurrentDateAsync(string portfolioId, CancellationToken cancellationToken = default);
}