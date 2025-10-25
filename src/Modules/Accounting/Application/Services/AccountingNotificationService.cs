using Accounting.Application.Abstractions;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Domain.Constants;
using Common.SharedKernel.Infrastructure.NotificationsCenter;
using Microsoft.Extensions.Configuration;

namespace Accounting.Application.Services;

public sealed class AccountingNotificationService(
    INotificationCenter notificationCenter,
    IConfiguration configuration) : IAccountingNotificationService
{
    private static string FormatDuration(double totalSeconds)
    {
        var duration = TimeSpan.FromSeconds(totalSeconds);

        if (duration.TotalDays >= 1)
            return $"{(int)duration.TotalDays} días";
        else if (duration.TotalHours >= 1)
            return $"{(int)duration.TotalHours} horas";
        else if (duration.TotalMinutes >= 1)
            return $"{(int)duration.TotalMinutes} minutos";
        else
            return $"{(int)totalSeconds} segundos";
    }
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
        CancellationToken cancellationToken = default)
    {
        var duration = (DateTime.UtcNow - startDate).TotalSeconds;
        var message = $"Proceso contable {processId} completado exitosamente para la fecha {processDate:yyyy-MM-dd}";

        var details = new Dictionary<string, string>
        {
            { "Exitoso", message },
            { "Duracion", FormatDuration(duration) },
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
        var duration = (DateTime.UtcNow - startDate).TotalSeconds;
        var details = new Dictionary<string, string>
        {
            { "Error", errorMessage },
            { "Duracion", FormatDuration(duration) },
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
        var duration = (DateTime.UtcNow - startDate).TotalSeconds;
        var details = new Dictionary<string, string>
        {
            { "url", reportUrl },
            { "Duracion", FormatDuration(duration) },
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
        IEnumerable<object> errors,
        int totalRecords,
        CancellationToken cancellationToken = default)
    {
        var duration = (DateTime.UtcNow - startDate).TotalSeconds;

        var details = new
        {
            Duracion = FormatDuration(duration),
            FechaGeneracion = processDate.ToString("yyyy-MM-dd"),
            RegistrosTotales = totalRecords,
            Errores = errors
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
}
