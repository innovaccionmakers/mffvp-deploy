
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Domain;

namespace Closing.Application.Closing.Services.PortfolioValuation;

public interface IPortfolioValuationService
{
    Task<Result> CalculateAndPersistValuationAsync(int portfolioId, DateTime closingDate, CancellationToken ct);

}
