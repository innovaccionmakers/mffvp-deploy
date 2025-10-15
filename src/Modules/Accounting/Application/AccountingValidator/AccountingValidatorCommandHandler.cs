using Accounting.Application.AccountProcess;
using Accounting.Integrations.AccountingValidator;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using MediatR;

namespace Accounting.Application.AccountingValidator;

internal sealed class AccountingValidatorCommandHandler(IAccountingProcessStore processStore) : ICommandHandler<AccountingValidatorCommand, Unit>
{
    public async Task<Result<Unit>> Handle(AccountingValidatorCommand request, CancellationToken cancellationToken)
    {
        await processStore.RegisterProcessResultAsync(request.ProcessId, request.ProcessType, request.IsSuccess, request.ErrorMessage, cancellationToken);

        var allProcessesCompleted = await processStore.AllProcessesCompletedAsync(request.ProcessId, cancellationToken);

        if (allProcessesCompleted)
        {
            var results = await processStore.GetAllProcessResultsAsync(request.ProcessId, cancellationToken);
            
            var allSuccessful = results.All(r => r.IsSuccess);
            var hasErrors = results.Any(r => !r.IsSuccess);


            await ExecuteFinalizationLogicAsync(request.ProcessId, request.ProcessDate, request.PortfolioIds, results, allSuccessful, hasErrors, cancellationToken);

            await processStore.CleanupAsync(request.ProcessId, cancellationToken);
        }

        return Unit.Value;
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

