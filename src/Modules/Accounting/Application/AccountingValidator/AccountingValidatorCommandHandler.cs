using Accounting.Application.AccountingValidator.Reports;
using Accounting.Application.AccountProcess;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Integrations.AccountingValidator;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.AccountingValidator;

internal sealed class AccountingValidatorCommandHandler(IAccountingProcessStore processStore,
                                                        IAccountingInconsistencyRepository accountingInconsistencyRepository,
                                                        IFileStorageService fileStorageService,
                                                        IServiceProvider serviceProvider,
                                                        ILogger<AccountingValidatorCommandHandler> logger) : ICommandHandler<AccountingValidatorCommand, Unit>
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
            if(allInconsistencies.Count != 0)
            {
                var url = await GenerateAccountingInconsistenciesUrl(processDate, allInconsistencies, cancellationToken);
                

            }
        }
    }

    private async Task<string> GenerateAccountingInconsistenciesUrl(DateTime processDate, IEnumerable<AccountingInconsistency> inconsistencies, CancellationToken cancellationToken)
    {
        try
        {
            var provider = serviceProvider.GetRequiredService<AccountingInconsistenciesReport>();
            var reportRequest = new AccountingInconsistenciesRequest
            {
                ProcessDate = processDate,
                Inconsistencies = inconsistencies
            };

            var fileResult = await provider!.GetReportDataAsync(reportRequest, cancellationToken);

            if (fileResult is FileStreamResult fileStreamResult)
            {
                using var memoryStream = new MemoryStream();
                await fileStreamResult.FileStream.CopyToAsync(memoryStream, cancellationToken);
                var fileBytes = memoryStream.ToArray();

                var fileName = fileStreamResult.FileDownloadName;
                
                var filePath = $"reports/inconsistencies/{fileName}";
                return await fileStorageService.UploadFileAsync(fileBytes, fileName, fileStreamResult.ContentType, filePath, cancellationToken);               
            }
            return string.Empty;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al generar el reporte de inconsistencias");
            return string.Empty;
        }
    }    
}

