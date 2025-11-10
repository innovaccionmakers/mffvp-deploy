using Accounting.Application.Abstractions;
using Accounting.Domain.Constants;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Helpers.Time;
using Common.SharedKernel.Domain.Constants;
using Common.SharedKernel.Domain.NotificationsCenter;
using Common.SharedKernel.Infrastructure.NotificationsCenter;
using Microsoft.Extensions.Configuration;

namespace Accounting.Application.Services;

public sealed class AccountingNotificationService(
    INotificationCenter notificationCenter,
    IConfiguration configuration) : IAccountingNotificationService
{
    public async Task SendNotificationAsync(
        string user,
        string? email,
        string status,
        string processId,
        string stepDescription,
        object details,
        string stepId,
        CancellationToken cancellationToken = default)
    {
        var administrator = configuration["NotificationSettings:Administrator"] ?? NotificationDefaults.Administrator;
        var emailFrom = configuration["NotificationSettings:EmailFrom"] ?? NotificationDefaults.EmailFrom;

        var recipients = new List<Recipient>();
        if (!string.IsNullOrWhiteSpace(email))
        {
            recipients.Add(new Recipient
            {
                Email = email,
                Usuario = user
            });
        }

        var buildMessage = NotificationCenter.BuildMessageBody(
            processId,
            stepId,
            administrator,
            NotificationTypes.AccountingReport,
            NotificationTypes.Report,
            status,
            stepDescription,
            details,
            true,
            emailFrom,
            recipients
        );

        await notificationCenter.SendNotificationAsync(user, buildMessage, cancellationToken);
    }

    public async Task SendProcessInitiatedAsync(
        string user,
        string? email,
        string processId,
        DateTime processDate,
        CancellationToken cancellationToken = default,
        string stepId = "1")
    {
        var details = new Dictionary<string, string>
        {
            { "Exitoso", $"Proceso contable {processId} iniciado exitosamente." },
            { "Fecha Generacion", processDate.ToString("yyyy-MM-dd") }
        };

        await SendNotificationAsync(
            user,
            email,
            NotificationStatuses.Initiated,
            processId,
            NotificationTypes.ReportGeneration,
            details,
            stepId,
            cancellationToken
        );
    }

    public async Task SendProcessFinalizedAsync(
        string user,
        string? email,
        string processId,
        DateTime startDate,
        DateTime processDate,
        string reportUrl,
        CancellationToken cancellationToken = default,
        string stepId = "2")
    {
        var message = $"Se proceso exitosamente la solicitud {processId}";

        var details = new Dictionary<string, string>
        {
            { "url", reportUrl },
            { "Exitoso", message },
            { "Duración", TimeHelper.GetDuration(startDate, DateTime.UtcNow) },
            { "Fecha Generacion", processDate.ToString("yyyy-MM-dd") }
        };

        await SendNotificationAsync(
            user,
            email,
            NotificationStatuses.Finalized,
            processId,
            NotificationTypes.ReportGeneration,
            details,
            stepId,
            cancellationToken
        );
    }

    public async Task SendProcessFailedAsync(
        string user,
        string? email,
        string processId,
        DateTime startDate,
        DateTime processDate,
        string errorMessage,
        CancellationToken cancellationToken = default,
        string stepId = "2")
    {
        var details = new Dictionary<string, string>
        {
            { "Error", errorMessage },
            { "Duración", TimeHelper.GetDuration(startDate, DateTime.UtcNow) },
            { "Fecha Generacion", processDate.ToString("yyyy-MM-dd") },
        };

        await SendNotificationAsync(
            user,
            email,
            NotificationStatuses.Failure,
            processId,
            NotificationTypes.ReportGeneratedError,
            details,
            stepId,
            cancellationToken
        );
    }

    public async Task SendProcessFailedWithUrlAsync(
        string user,
        string email,
        string processId,
        DateTime startDate,
        DateTime processDate,
        string reportUrl,
        int totalRecords,
        CancellationToken cancellationToken = default,
        string stepId = "2")
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
            email,
            NotificationStatuses.Failure,
            processId,
            NotificationTypes.ReportGeneratedError,
            details,
            stepId,
            cancellationToken
        );
    }

    public async Task SendProcessFailedWithErrorsAsync(
        string user,
        string? email,
        string processId,
        DateTime startDate,
        DateTime processDate,
        IEnumerable<UndefinedError> errors,
        CancellationToken cancellationToken = default,
        string stepId = "2")
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
            email,
            NotificationStatuses.Failure,
            processId,
            NotificationTypes.ReportGeneratedError,
            details,
            stepId,
            cancellationToken
        );
    }
}
