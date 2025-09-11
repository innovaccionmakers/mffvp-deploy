namespace Operations.Application.Abstractions.Services.Portfolio;

public interface IPortfolioService
{
    Task<DateTime> GetCurrentDateAsync(string portfolioId, int objetiveId, CancellationToken cancellationToken = default);

    Task<DateTime> GetNextDateFromCurrentDateAsync(string portfolioId, int objetiveId, CancellationToken cancellationToken = default);
}