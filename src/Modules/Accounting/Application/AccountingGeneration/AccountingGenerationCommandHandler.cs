using Accounting.Application.Abstractions;
using Accounting.Application.AccountingGeneration.Reports;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.Consecutives;
using Accounting.Integrations.AccountingGeneration;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Accounting.Application.AccountingGeneration;

internal sealed class AccountingGenerationCommandHandler(IAccountingAssistantRepository accountingAssistantRepository,
                                                         IConsecutiveRepository consecutiveRepository,
                                                         ILogger<AccountingGenerationCommandHandler> logger,
                                                         IFileStorageService fileStorageService,
                                                         IServiceProvider serviceProvider,
                                                         IAccountingNotificationService accountingNotificationService) : ICommandHandler<AccountingGenerationCommand, Unit>
{
    public async Task<Result<Unit>> Handle(AccountingGenerationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var accountingAssistants = await accountingAssistantRepository.GetAllAsync(cancellationToken);
            if(accountingAssistants.Count == 0)
            {
                await accountingNotificationService.SendProcessFailedAsync(request.User, request.Email, request.ProcessId, request.StartDate, request.ProcessDate, "No existen registros contables para generar.", cancellationToken);
                return Unit.Value;
            }

            var validationError = AccountingGenerationValidator.ValidateNatureRecordLimits(accountingAssistants);
            if (validationError is not null)
            {
                await accountingNotificationService.SendProcessFailedAsync(request.User, request.Email, request.ProcessId, request.StartDate, request.ProcessDate, validationError, cancellationToken);
                return Unit.Value;
            }

            var consecutives = await consecutiveRepository.GetAllAsync(cancellationToken);

            var consecutiveValidationError = AccountingGenerationValidator.ValidateConsecutivesExist(consecutives);
            if (consecutiveValidationError is not null)
            {
                await accountingNotificationService.SendProcessFailedAsync(request.User, request.Email, request.ProcessId, request.StartDate, request.ProcessDate, consecutiveValidationError, cancellationToken);
                return Unit.Value;
            }

            var url = await GenerateAccountingInterfaceUrl(request.ProcessDate, accountingAssistants, consecutives, cancellationToken);

            if(url.IsNullOrEmpty())
            {
                await accountingNotificationService.SendProcessFailedAsync(request.User, request.Email, request.ProcessId, request.StartDate, request.ProcessDate, "Hubo un error al generar la interfaz contable.", cancellationToken);
                return Unit.Value;
            }

            await accountingNotificationService.SendProcessFinalizedAsync(request.User, request.Email, request.ProcessId, request.StartDate, request.ProcessDate, url, cancellationToken);
        }
        catch (Exception ex)
        {
            var error = "Ocurrio un error inesperado al generar la interface contable";
            logger.LogError(ex, error);
            await accountingNotificationService.SendProcessFailedAsync(request.User, request.Email, request.ProcessId, request.StartDate, request.ProcessDate, error, cancellationToken);

        }
        return Result.Success(Unit.Value);
    }

    private async Task<string> GenerateAccountingInterfaceUrl(DateTime processDate, IEnumerable<AccountingAssistant> accountingAssistants, IEnumerable<Consecutive> consecutives, CancellationToken cancellationToken)
    {
        try
        {
            var report = serviceProvider.GetRequiredService<AccountingGenerationReport>();

            var fileStreamResult = await report.GenerateReportAsync(processDate, accountingAssistants, consecutives, cancellationToken);

            using var memoryStream = new MemoryStream();
            await fileStreamResult.FileStream.CopyToAsync(memoryStream, cancellationToken);
            var fileBytes = memoryStream.ToArray();


            string fileName = fileStreamResult.FileDownloadName;

            var filePath = $"reports/accounting/{fileName}";
            return await fileStorageService.UploadFileAsync(fileBytes, fileName, fileStreamResult.ContentType, filePath, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al generar el reporte de generación contable");
            return string.Empty;
        }
    }
}
