using Common.SharedKernel.Domain;

namespace Closing.Application.Abstractions.External;

public interface IPortfolioValidator
{
    Task<Result> EnsureExistsAsync(int portfolioId, CancellationToken ct);
}