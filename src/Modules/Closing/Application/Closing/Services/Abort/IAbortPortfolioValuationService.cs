
namespace Closing.Application.Closing.Services.Abort;

public interface IAbortPortfolioValuationService
{
    /// <summary>
    /// Borra los registros cerrados (cerrado=true) de Portfolio Valuation
    /// para un portafolio y fecha de cierre.
    /// </summary>
    Task DeleteClosedValuationAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken);
}