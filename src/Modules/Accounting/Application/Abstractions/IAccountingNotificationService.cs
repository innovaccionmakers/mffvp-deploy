namespace Accounting.Application.Abstractions;

public record UndefinedError(string ProcessType, string ErrorDescription);

public interface IAccountingNotificationService
{
     Task SendNotificationAsync(
     string user,
     string email,
     string status,
     string processId,
     string stepDescription,
     object details,
     string stepId,
     CancellationToken cancellationToken = default);

 Task SendProcessInitiatedAsync(
     string user,
     string? email,
     string processId,
     DateTime processDate,
     CancellationToken cancellationToken = default,
     string stepId = "1");

 Task SendProcessFinalizedAsync(
     string user,
     string? email,
     string processId,
     DateTime startDate,
     DateTime processDate,
     string reportUrl,
     CancellationToken cancellationToken = default,
     string stepId = "2");

Task SendProcessFailedAsync(
    string user,
    string? email,
    string processId,
    DateTime startDate,
    DateTime processDate,
    string errorMessage,
    CancellationToken cancellationToken = default,
    string stepId = "2");

Task SendProcessFailedWithUrlAsync(
    string? user,
    string email,
    string processId,
    DateTime startDate,
    DateTime processDate,
    string reportUrl,
    int totalRecords,
    CancellationToken cancellationToken = default,
    string stepId = "2");

Task SendProcessFailedWithErrorsAsync(
    string user,
    string? email,
    string processId,
    DateTime startDate,
    DateTime processDate,
    IEnumerable<UndefinedError> errors,
    CancellationToken cancellationToken = default,
    string stepId = "2");
}

