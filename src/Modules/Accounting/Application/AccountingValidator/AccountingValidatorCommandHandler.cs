using Accounting.Application.Abstractions;
using Accounting.Application.AccountingValidator.Reports;
using Accounting.Application.AccountProcess;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Integrations.AccountingValidator;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.AccountingValidator;

internal sealed class AccountingValidatorCommandHandler(IAccountingProcessStore processStore,
                                                        IAccountingInconsistencyRepository accountingInconsistencyRepository,
                                                        IFileStorageService fileStorageService,
                                                        IServiceProvider serviceProvider,
                                                        IAccountingNotificationService accountingNotificationService,
                                                        ILogger<AccountingValidatorCommandHandler> logger) : ICommandHandler<AccountingValidatorCommand, Unit>
{
    public async Task<Result<Unit>> Handle(AccountingValidatorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await processStore.RegisterProcessResultAsync(request.ProcessId, request.ProcessType, request.IsSuccess, request.ErrorMessage, cancellationToken);

            var allProcessesCompleted = await processStore.AllProcessesCompletedAsync(request.ProcessId, cancellationToken);

            if (allProcessesCompleted)
            {
                var results = await processStore.GetAllProcessResultsAsync(request.ProcessId, cancellationToken);

                var hasErrors = results.Any(r => !r.IsSuccess);


                await ExecuteFinalizationLogicAsync(request.User, request.ProcessId, request.ProcessDate, results, hasErrors, cancellationToken);

                await processStore.CleanupAsync(request.ProcessId, cancellationToken);
            }

        }catch(Exception ex)
        {
            var error = "Ocurrió un error inesperado el completar la validación del proceso contable";
            logger.LogError(ex, error);
            await accountingNotificationService.SendProcessFailedAsync(request.User, request.ProcessId, error, cancellationToken);
        }
        return Unit.Value;
    }

    private async Task ExecuteFinalizationLogicAsync(
        string user,
        string processId,
        DateTime processDate,
        List<ProcessResult> results,
        bool hasErrors,
        CancellationToken cancellationToken)
    {
        if (!hasErrors)
        {
            await accountingNotificationService.SendProcessStatusAsync(user, processId.ToString(), processDate, NotificationStatuses.Finalized, cancellationToken);
            return;
        }

        // Procesar errores
        var allInconsistencies = new List<AccountingInconsistency>();
        var undefinedErrors = new List<object>();
        var failedProcesses = results.Where(r => !r.IsSuccess).ToList();

        foreach (var failed in failedProcesses)
        {
            var inconsistenciesResult = await accountingInconsistencyRepository.GetInconsistenciesAsync(
                processDate,
                failed.ProcessType,
                cancellationToken);

            if (!inconsistenciesResult.Value.Any())
            {
                undefinedErrors.Add(new { failed.ProcessType, ErrorDescription = failed.ErrorMessage });
                continue;
            }

            allInconsistencies.AddRange(inconsistenciesResult.Value);
        }

        if (undefinedErrors.Count > 0)
        {
            await accountingNotificationService.SendProcessFailedWithErrorsAsync(user, processId, undefinedErrors, cancellationToken);
            return;
        }

        if (allInconsistencies.Count > 0)
        {
            var url = await GenerateAccountingInconsistenciesUrl(processDate, allInconsistencies, cancellationToken);
            await accountingNotificationService.SendProcessFailedWithUrlAsync(user, processId, url, cancellationToken);
            return;
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

