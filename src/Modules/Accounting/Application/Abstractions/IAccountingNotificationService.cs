namespace Accounting.Application.Abstractions;

public record UndefinedError(string ProcessType, string ErrorDescription);

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

 Task SendProcessFinalizedAsync(
     string user,
     string processId,
     DateTime startDate,
     DateTime processDate,
     CancellationToken cancellationToken = default);

Task SendProcessFailedAsync(
    string user,
    string processId,
    DateTime startDate,
    DateTime processDate,
    string errorMessage,
    CancellationToken cancellationToken = default);

Task SendProcessFailedWithUrlAsync(
    string user,
    string processId,
    DateTime startDate,
    DateTime processDate,
    string reportUrl,
    int totalRecords,
    CancellationToken cancellationToken = default);

Task SendProcessFailedWithErrorsAsync(
    string user,
    string processId,
    DateTime startDate,
    DateTime processDate,
    IEnumerable<UndefinedError> errors,
    int totalRecords,
    CancellationToken cancellationToken = default);
}

