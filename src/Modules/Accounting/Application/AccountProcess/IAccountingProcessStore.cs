namespace Accounting.Application.AccountProcess;

public interface IAccountingProcessStore
{
    Task RegisterProcessResultAsync(Guid processId, string processType, bool isSuccess, string? errorMessage, CancellationToken cancellationToken);
    Task<bool> AreAllProcessesCompletedAsync(Guid processId, CancellationToken cancellationToken);
    Task<List<ProcessResult>> GetAllProcessResultsAsync(Guid processId, CancellationToken cancellationToken);
    Task CleanupAsync(Guid processId, CancellationToken cancellationToken);
}

public record ProcessResult(string ProcessType, bool IsSuccess, string? ErrorMessage);
