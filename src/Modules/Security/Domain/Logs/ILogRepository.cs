namespace Security.Domain.Logs;

public interface ILogRepository
{
    Task<IReadOnlyCollection<Log>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Log?> GetAsync(long logId, CancellationToken cancellationToken = default);
    void Insert(Log log);
    void Update(Log log);
    void Delete(Log log);
}
