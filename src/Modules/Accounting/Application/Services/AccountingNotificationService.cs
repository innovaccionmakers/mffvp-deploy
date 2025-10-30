using Accounting.Application.Abstractions;
using Accounting.Domain.Constants;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Helpers.Time;
using Common.SharedKernel.Domain.Constants;
using Common.SharedKernel.Infrastructure.NotificationsCenter;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.Extensions.Configuration;

namespace Accounting.Application.Services;

public sealed class AccountingNotificationService(
    INotificationCenter notificationCenter,
    IConfiguration configuration) : IAccountingNotificationService
{
    public async Task SendNotificationAsync(
        string user,
        string status,
        string processId,
        string stepDescription,
        object details,
        CancellationToken cancellationToken = default)
    {
        var administrator = configuration["NotificationSettings:Administrator"] ?? NotificationDefaults.Administrator;

        var buildMessage = NotificationCenter.BuildMessageBody(
            processId,
            Guid.NewGuid().ToString(),
            administrator,
            NotificationTypes.AccountingReport,
            NotificationTypes.Report,
            status,
            stepDescription,
            details
        );

        await notificationCenter.SendNotificationAsync(user, buildMessage, cancellationToken);
    }

    public async Task SendProcessInitiatedAsync(
        string user,
        string processId,
        DateTime processDate,
        CancellationToken cancellationToken = default)
    {
        var details = new Dictionary<string, string>
        {
            { "Exitoso", $"Proceso contable {processId} iniciado exitosamente." },
            { "Fecha Generacion", processDate.ToString("yyyy-MM-dd") }
        };

        await SendNotificationAsync(
            user,
            NotificationStatuses.Initiated,
            processId,
            NotificationTypes.ReportGeneration,
            details,
            cancellationToken
        );
    }

    public async Task SendProcessFinalizedAsync(
        string user,
        string processId,
        DateTime startDate,
        DateTime processDate,
        string reportUrl,
        CancellationToken cancellationToken = default)
    {
        var message = $"Proceso contable {processId} completado exitosamente para la fecha {processDate:yyyy-MM-dd}";

        var details = new Dictionary<string, string>
        {
            { "url", reportUrl },
            { "Exitoso", message },
            { "Duración", TimeHelper.GetDuration(startDate, DateTime.UtcNow) },
            { "Fecha Generacion", processDate.ToString("yyyy-MM-dd") }
        };

        await SendNotificationAsync(
            user,
            NotificationStatuses.Finalized,
            processId,
            NotificationTypes.ReportGeneration,
            details,
            cancellationToken
        );
    }

    public async Task SendProcessFailedAsync(
        string user,
        string processId,
        DateTime startDate,
        DateTime processDate,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {        
        var details = new Dictionary<string, string>
        {
            { "Error", errorMessage },
            { "Duración", TimeHelper.GetDuration(startDate, DateTime.UtcNow) },
            { "Fecha Generacion", processDate.ToString("yyyy-MM-dd") },
        };

        await SendNotificationAsync(
            user,
            NotificationStatuses.Failure,
            processId,
            NotificationTypes.ReportGeneratedError,
            details,
            cancellationToken
        );
    }

    public async Task SendProcessFailedWithUrlAsync(
        string user,
        string processId,
        DateTime startDate,
        DateTime processDate,
        string reportUrl,
        int totalRecords,
        CancellationToken cancellationToken = default)
    {        
        var details = new Dictionary<string, string>
        {
            { "url", reportUrl },
            { "Duración", TimeHelper.GetDuration(startDate, DateTime.UtcNow) },
            { "Fecha Generacion", processDate.ToString("yyyy-MM-dd") },
            { "Registros Totales", totalRecords.ToString() }
        };

        await SendNotificationAsync(
            user,
            NotificationStatuses.Failure,
            processId,
            NotificationTypes.ReportGeneratedError,
            details,
            cancellationToken
        );
    }

    public async Task SendProcessFailedWithErrorsAsync(
        string user,
        string processId,
        DateTime startDate,
        DateTime processDate,
        IEnumerable<UndefinedError> errors,
        CancellationToken cancellationToken = default)
    {
        var errorsList = errors.ToList();
        var details = new Dictionary<string, object>
        {
            { "Duracion", TimeHelper.GetDuration(startDate, DateTime.UtcNow) },
            { "FechaGeneracion", processDate.ToString("yyyy-MM-dd") },
        };

        foreach (var error in errorsList)
        {
            var translatedProcessType = ProcessTypes.GetTranslation(error.ProcessType);
            details[$"Error {translatedProcessType}"] = error.ErrorDescription;
        }

        await SendNotificationAsync(
            user,
            NotificationStatuses.Failure,
            processId,
            NotificationTypes.ReportGeneratedError,
            details,
            cancellationToken
        );
    }
}
