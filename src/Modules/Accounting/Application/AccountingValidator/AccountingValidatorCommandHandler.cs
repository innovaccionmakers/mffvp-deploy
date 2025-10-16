using Accounting.Application.AccountProcess;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Integrations.AccountingValidator;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using MediatR;

namespace Accounting.Application.AccountingValidator;

internal sealed class AccountingValidatorCommandHandler(IAccountingProcessStore processStore, IAccountingInconsistencyRepository accountingInconsistencyRepository) : ICommandHandler<AccountingValidatorCommand, Unit>
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
        if (allSuccessful)
        {
            // Todos los procesos fueron exitosos
            Console.WriteLine($"Proceso contable {processId} completado exitosamente para fecha {processDate:yyyy-MM-dd}");
        }
        else if (hasErrors)
        {
            var allInconsistencies = new List<AccountingInconsistency>();
            var failedProcesses = results.Where(r => !r.IsSuccess).ToList();
            Console.WriteLine($"Proceso contable {processId} completado con errores:");

            foreach (var failed in failedProcesses)
            {
                var inconsistenciesResult = await accountingInconsistencyRepository.GetInconsistenciesAsync(
                    processDate,
                    failed.ProcessType,
                    cancellationToken);

                if(inconsistenciesResult.Value.Any())
                {
                    allInconsistencies.AddRange(inconsistenciesResult.Value);
                }
                else
                {
                    Console.WriteLine($"- {failed.ProcessType}: {failed.ErrorMessage}");
                }

            }
            if(allInconsistencies.Any())
            {
                Console.WriteLine($"Se encontraron {allInconsistencies.Count} inconsistencias:");
                foreach (var inconsistency in allInconsistencies)
                {
                    Console.WriteLine($"- {inconsistency.Inconsistency}");
                }
            }
        }
    }
}

