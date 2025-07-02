using Common.SharedKernel.Domain;

namespace Closing.Application.Abstractions.External;

public interface IPortfolioValidator
{
    Task<Result> EnsureExistsAsync(int portfolioId, CancellationToken ct);
    Task<Result<PortfolioData>> GetPortfolioDataAsync(int portfolioId, CancellationToken ct);
}

public readonly record struct PortfolioData(
    int PortfolioId,
    DateTime CurrentDate
);