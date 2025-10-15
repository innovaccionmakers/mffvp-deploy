using Accounting.IntegrationEvents.AccountingProcess;
using DotNetCore.CAP;

namespace Accounting.Application.AccountProcess;

public sealed class AccountingProcessCoordinatorConsumer(IAccountingProcessStore processStore) : ICapSubscribe
{

    [CapSubscribe(nameof(AccountingProcessCompletedIntegrationEvent))]
    public async Task HandleAsync(AccountingProcessCompletedIntegrationEvent evt, CancellationToken cancellationToken)
    {
        await processStore.RegisterProcessResultAsync(evt.ProcessId, evt.ProcessType, evt.IsSuccess, evt.ErrorMessage, cancellationToken);

        var allProcessesCompleted = await processStore.AreAllProcessesCompletedAsync(evt.ProcessId, cancellationToken);

        if (allProcessesCompleted)
        {
            var results = await processStore.GetAllProcessResultsAsync(evt.ProcessId, cancellationToken);

            // Determinar el estado general
            var allSuccessful = results.All(r => r.IsSuccess);
            var hasErrors = results.Any(r => !r.IsSuccess);

            // Ejecutar la lógica de finalización
            await ExecuteFinalizationLogicAsync(evt.ProcessId, evt.ProcessDate, evt.PortfolioIds, results, allSuccessful, hasErrors, cancellationToken);

            // Limpiar el store
            await processStore.CleanupAsync(evt.ProcessId, cancellationToken);
        }
    }

    private async Task ExecuteFinalizationLogicAsync(
        Guid processId,
        DateTime processDate,
        IEnumerable<int> portfolioIds,
        List<ProcessResult> results,
        bool allSuccessful,
        bool hasErrors,
        CancellationToken cancellationToken)
    {
        // Aquí implementas la lógica que estaba en el comentario del ExecuteAccountingOperationAsync

        if (allSuccessful)
        {
            // Todos los procesos fueron exitosos
            // Implementar lógica de éxito
            Console.WriteLine($"Proceso contable {processId} completado exitosamente para fecha {processDate:yyyy-MM-dd}");
        }
        else if (hasErrors)
        {
            // Algunos procesos fallaron
            var failedProcesses = results.Where(r => !r.IsSuccess).ToList();
            Console.WriteLine($"Proceso contable {processId} completado con errores:");

            foreach (var failed in failedProcesses)
            {
                Console.WriteLine($"- {failed.ProcessType}: {failed.ErrorMessage}");
            }
        }

        // Aquí puedes agregar más lógica como:
        // - Notificar al usuario
        // - Actualizar base de datos
        // - Enviar emails
        // - etc.
    }
}
