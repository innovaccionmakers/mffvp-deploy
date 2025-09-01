
namespace Closing.Application.Closing.Services.Abort;

public interface IAbortSimulationService
{
    /// <summary>
    /// Borra los registros cerrados (cerrado=true) de Yields y YieldDetails
    /// para un portafolio y fecha de cierre.
    /// </summary>
    Task DeleteClosedSimulationAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken);
}