using Accounting.Application.Abstractions;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.AccountingInconsistencies;
using Microsoft.Extensions.Logging;

namespace Accounting.Application;

/// <summary>
/// Implementación del manejo de inconsistencias para el módulo de Accounting
/// </summary>
public sealed class InconsistencyHandler(
    IAccountingInconsistencyRepository accountingInconsistencyRepository,
    ILogger<InconsistencyHandler> logger) : IInconsistencyHandler
{
    public async Task HandleInconsistenciesAsync(IEnumerable<AccountingInconsistency> inconsistencies, DateTime processDate, string processType, CancellationToken cancellationToken = default)
    {
        try
        {
            var automaticConceptErrors = inconsistencies
                .DistinctBy(e => new {
                    e.PortfolioId,
                    e.Transaction,
                    e.Inconsistency,
                    e.Activity
                })
                .ToList();

            logger.LogWarning("Se detectaron inconsistencias en el proceso {ProcessType} para la fecha {ProcessDate}",
                processType, processDate);

            var result = await accountingInconsistencyRepository.SaveInconsistenciesAsync(
                automaticConceptErrors, processDate, processType, cancellationToken);

            if (result.IsFailure)
            {
                logger.LogError("Error al procesar inconsistencias: {Error}", result.Error);
            }
            else
            {
                logger.LogInformation("Inconsistencias procesadas exitosamente para el proceso {ProcessType} en la fecha {ProcessDate}",
                    processType, processDate);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al manejar las inconsistencias para {ProcessType}",
                processType);
        }
    }
}
