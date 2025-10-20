using Accounting.Application.AccountingValidator.Reports;
using Accounting.Application.AccountProcess;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Integrations.AccountingValidator;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.Constants;
using Common.SharedKernel.Infrastructure.NotificationsCenter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Accounting.Application.AccountingValidator;

internal sealed class AccountingValidatorCommandHandler(IAccountingProcessStore processStore,
                                                        IAccountingInconsistencyRepository accountingInconsistencyRepository,
                                                        IFileStorageService fileStorageService,
                                                        IServiceProvider serviceProvider,
                                                        INotificationCenter notificationCenter,
                                                        IConfiguration configuration,
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


            await ExecuteFinalizationLogicAsync(request.User, request.ProcessId, request.ProcessDate, request.PortfolioIds, results, allSuccessful, hasErrors, cancellationToken);

            await processStore.CleanupAsync(request.ProcessId, cancellationToken);
        }

        return Unit.Value;
    }

    private async Task ExecuteFinalizationLogicAsync(
        string user,
        Guid processId,
        DateTime processDate,
        IEnumerable<int> portfolioIds,
        List<ProcessResult> results,
        bool allSuccessful,
        bool hasErrors,
        CancellationToken cancellationToken)
    {
        if (!hasErrors)
        {
            await notificationCenter.SendNotificationAsync(user, $"Proceso contable {processId} completado exitosamente para fecha {processDate:yyyy-MM-dd}", cancellationToken);
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
            await SendNotification(user, NotificationStatuses.Failure, processId.ToString(), undefinedErrors, cancellationToken);
            return;
        }
        
        if (allInconsistencies.Count > 0)
        {
            var url = await GenerateAccountingInconsistenciesUrl(processDate, allInconsistencies, cancellationToken);
            await SendNotification(user, NotificationStatuses.Finalized, processId.ToString(), new Dictionary<string, string> { { "url", url } }, cancellationToken);
            return;
        }
    }


    private async Task SendNotification(string user, string status, string processId, object details, CancellationToken cancellationToken)
    {
        var administrator = configuration["NotificationSettings:Administrator"] ?? NotificationDefaults.Administrator;

        var buildMessage = NotificationCenter.BuildMessageBody(
            processId,
            processId,
            administrator,
            NotificationTypes.AccountingReport,
            NotificationTypes.Report,
            status,
            NotificationTypes.ReportGeneration,
            details
        );
        await notificationCenter.SendNotificationAsync(user, buildMessage, cancellationToken);

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

