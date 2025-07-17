using Common.SharedKernel.Domain;
namespace Treasury.Application.Abstractions.External;

public interface IPortfolioValuationLocator
{
    Task<Result<bool>> CheckPortfolioValuationExists(DateTime closingDate, CancellationToken ct);
}
