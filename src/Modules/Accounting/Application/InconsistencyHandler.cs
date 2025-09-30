using Accounting.Application.Abstractions;
using Accounting.Domain.AccountingInconsistencies;
using Common.SharedKernel.Core.Primitives;
using Microsoft.Extensions.Logging;

namespace Accounting.Application;

/// <summary>
/// Implementación del manejo de inconsistencias para el módulo de Accounting
/// </summary>
public sealed class InconsistencyHandler(ILogger<InconsistencyHandler> logger) : IInconsistencyHandler
{
    public async Task HandleInconsistenciesAsync(IEnumerable<AccountingInconsistency> inconsistencies, DateTime processDate, string processType, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogError("Error al crear las entidades contables: {Error}", inconsistencies);
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
