using Accounting.Application.Abstractions;
using Common.SharedKernel.Core.Primitives;
using Microsoft.Extensions.Logging;

namespace Accounting.Application;

/// <summary>
/// Implementación del manejo de inconsistencias para el módulo de Accounting
/// </summary>
internal sealed class InconsistencyHandler(ILogger<InconsistencyHandler> logger) : IInconsistencyHandler
{
    public async Task HandleInconsistenciesAsync(
        IEnumerable<Error> errors,
        IEnumerable<string> portfolioIds,
        DateTime processDate,
        string processType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            //TODO: un log
            // TODO: Implementar envío a Redis


            // Simular operación asíncrona
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al manejar las inconsistencias para {ProcessType} en los portafolios: {PortfolioIds}",
                processType, string.Join(", ", portfolioIds));
        }
    }
}
