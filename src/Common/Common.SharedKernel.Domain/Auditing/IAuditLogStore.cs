namespace Common.SharedKernel.Domain.Auditing
{
    public interface IAuditLogStore
    {
        Task SaveLogReferenceAsync(long id, string processId, CancellationToken cancellationToken = default);
        Task UpdateLogStatusAsync(string processId, CancellationToken cancellationToken = default);
        Task RemoveLogReferenceAsync(string processId, CancellationToken cancellationToken = default);
    }

    public record LogReference(long Id, string ProcessId);
}
