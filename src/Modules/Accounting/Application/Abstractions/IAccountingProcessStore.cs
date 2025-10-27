namespace Accounting.Application.AccountProcess;

public interface IAccountingProcessStore
{
    Task RegisterProcessResultAsync(string processId, string processType, bool isSuccess, string? errorMessage, CancellationToken cancellationToken);
    Task<bool> AllProcessesCompletedAsync(string processId, CancellationToken cancellationToken);
    Task<List<ProcessResult>> GetAllProcessResultsAsync(string processId, CancellationToken cancellationToken);
    Task CleanupAsync(string processId, CancellationToken cancellationToken);
}

public record ProcessResult(string ProcessType, bool IsSuccess, string? ErrorMessage);
