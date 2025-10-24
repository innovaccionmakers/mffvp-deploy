namespace Accounting.Application.Abstractions;

public interface IAccountingNotificationService
{
     Task SendNotificationAsync(
     string user,
     string status,
     string processId,
     string stepDescription,
     object details,
     CancellationToken cancellationToken = default);

 Task SendProcessInitiatedAsync(
     string user,
     string processId,
     DateTime processDate,
     CancellationToken cancellationToken = default);

 Task SendProcessStatusAsync(
     string user,
     string processId,
     DateTime startDate,
     DateTime processDate,
     string status,
     CancellationToken cancellationToken = default);

 Task SendProcessFailedAsync(
     string user,
     string processId,
     DateTime startDate,
     string errorMessage,
     CancellationToken cancellationToken = default);

 Task SendProcessFailedWithUrlAsync(
     string user,
     string processId,
     DateTime startDate,
     string reportUrl,
     CancellationToken cancellationToken = default);

 Task SendProcessFailedWithErrorsAsync(
     string user,
     string processId,
     DateTime startDate,
     IEnumerable<object> errors,
     CancellationToken cancellationToken = default);
}

