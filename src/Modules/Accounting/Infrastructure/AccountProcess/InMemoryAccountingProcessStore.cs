using Accounting.Application.AccountProcess;
using Accounting.Domain.Constants;

namespace Accounting.Infrastructure.AccountProcess;

internal sealed class InMemoryAccountingProcessStore : IAccountingProcessStore
{
    private readonly Dictionary<Guid, Dictionary<string, ProcessResult>> _processResults = new();
    private readonly object _lock = new object();

    public Task RegisterProcessResultAsync(Guid processId, string processType, bool isSuccess, string? errorMessage, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            if (!_processResults.ContainsKey(processId))
            {
                _processResults[processId] = new Dictionary<string, ProcessResult>();
            }

            _processResults[processId][processType] = new ProcessResult(processType, isSuccess, errorMessage);
        }

        return Task.CompletedTask;
    }

    public Task<bool> AreAllProcessesCompletedAsync(Guid processId, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            if (!_processResults.ContainsKey(processId))
                return Task.FromResult(false);

            var results = _processResults[processId];

            // Verificar que todos los tipos de proceso estén presentes
            var requiredProcessTypes = new[]
            {
                ProcessTypes.AccountingFees,
                ProcessTypes.AccountingReturns,
                ProcessTypes.AccountingOperations,
                ProcessTypes.AccountingConcepts
            };

            return Task.FromResult(requiredProcessTypes.All(type => results.ContainsKey(type)));
        }
    }

    public Task<List<ProcessResult>> GetAllProcessResultsAsync(Guid processId, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            if (!_processResults.ContainsKey(processId))
                return Task.FromResult(new List<ProcessResult>());

            return Task.FromResult(_processResults[processId].Values.ToList());
        }
    }

    public Task CleanupAsync(Guid processId, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            _processResults.Remove(processId);
        }

        return Task.CompletedTask;
    }
}
