using Accounting.Application.Abstractions;
using Accounting.Application.AccountingGeneration.Reports;
using Accounting.Application.AccountingValidator.Reports;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Integrations.AccountingGeneration;
using Azure.Core;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.AccountingGeneration;

internal sealed class AccountingGenerationCommandHandler(IAccountingAssistantRepository accountingAssistantRepository,
                                                         ILogger<AccountingGenerationCommandHandler> logger,
                                                         IFileStorageService fileStorageService,
                                                         IServiceProvider serviceProvider,
                                                         IAccountingNotificationService accountingNotificationService) : ICommandHandler<AccountingGenerationCommand, Unit>
{
    public async Task<Result<Unit>> Handle(AccountingGenerationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var url = await GenerateAccountingInterfaceUrl(request.ProcessDate, cancellationToken);

        }
        catch(Exception ex)
        {
            var error = "Ocurrio un error inesperado al generar la interface contable";
            logger.LogError(ex, error);
            await accountingNotificationService.SendProcessFailedAsync(request.User, request.ProcessId, request.StartDate, request.ProcessDate, error, cancellationToken);

        }
        return Result.Success(Unit.Value);
    }

    private async Task<string> GenerateAccountingInterfaceUrl(DateTime processDate, CancellationToken cancellationToken)
    {
        try
        {
            var provider = serviceProvider.GetRequiredService<AccountingGenerationReport>();

            var actionResult = await provider.GetReportDataAsync(processDate, cancellationToken);

            if (actionResult is not FileResult fileResult)
            {
                logger.LogError("El reporte no devolvió un FileResult válido");
                return string.Empty;
            }

            byte[] fileBytes;
            string fileName = fileResult.FileDownloadName ?? $"reporte-{processDate:yyyyMMddHHmmss}.txt";
            string contentType = fileResult.ContentType ?? "application/octet-stream";

            switch (actionResult)
            {
                case FileStreamResult fsr:
                {
                    // Garantizar posición al inicio antes de copiar
                    if (fsr.FileStream.CanSeek)
                    {
                        fsr.FileStream.Seek(0, SeekOrigin.Begin);
                    }
                    using var memoryStream = new MemoryStream();
                    await fsr.FileStream.CopyToAsync(memoryStream, cancellationToken);
                    fileBytes = memoryStream.ToArray();
                    break;
                }
                case FileContentResult fcr:
                {
                    fileBytes = fcr.FileContents;
                    break;
                }
                default:
                {
                    logger.LogError("Tipo de FileResult no soportado: {Type}", actionResult.GetType().Name);
                    return string.Empty;
                }
            }

            var filePath = $"reports/inconsistencies/{fileName}";
            return await fileStorageService.UploadFileAsync(fileBytes, fileName, contentType, filePath, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al generar el reporte de inconsistencias");
            return string.Empty;
        }
    }
}
