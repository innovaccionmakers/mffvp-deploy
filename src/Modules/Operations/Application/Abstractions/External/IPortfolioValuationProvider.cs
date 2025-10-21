using Common.SharedKernel.Domain;

namespace Operations.Application.Abstractions.External;

public interface IPortfolioValuationProvider
{
    Task<Result<decimal>> GetUnitValueAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken);
}
