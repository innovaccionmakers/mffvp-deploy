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

    public async Task SendProcessStatusAsync(
        string user,
        string processId,
        DateTime processDate,
        string status,
        CancellationToken cancellationToken = default)
    {
        var message = status == NotificationStatuses.Initiated
            ? $"Proceso contable {processId} iniciado exitosamente para fecha {processDate:yyyy-MM-dd}"
            : $"Proceso contable {processId} completado exitosamente para la fecha {processDate:yyyy-MM-dd}";

        var details = new Dictionary<string, string>
        {
            { "Exitoso", message }
        };

        await SendNotificationAsync(
            user,
            status,
            processId,
            NotificationTypes.ReportGeneration,
            details,
            cancellationToken
        );
    }

    public async Task SendProcessFailedAsync(
        string user,
        string processId,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        var details = new Dictionary<string, string>
        {
            { "Error", errorMessage }
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
        string reportUrl,
        CancellationToken cancellationToken = default)
    {
        var details = new Dictionary<string, string>
        {
            { "url", reportUrl }
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
        IEnumerable<object> errors,
        CancellationToken cancellationToken = default)
    {
        await SendNotificationAsync(
            user,
            NotificationStatuses.Failure,
            processId,
            NotificationTypes.ReportGeneratedError,
            errors,
            cancellationToken
        );
    }
}
