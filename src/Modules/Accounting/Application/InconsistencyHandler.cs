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
        DateTime processDate,
        string processType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogError("Error al crear las entidades contables: {Error}", errors);
            // TODO: Implementar envío a Redis


            // Simular operación asíncrona
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al manejar las inconsistencias para {ProcessType}",
                processType);
        }
    }
}
