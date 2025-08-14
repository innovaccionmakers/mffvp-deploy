namespace Common.SharedKernel.Application.Caching.Closing.Interfaces;

public interface IClosingExecutionStore
{
    Task<bool> IsClosingActiveAsync(CancellationToken cancellationToken = default);

    Task BeginAsync(DateTime closingBeginTime, CancellationToken cancellationToken = default);

    Task UpdateProcessAsync(string process, DateTime processDatetime, CancellationToken cancellationToken = default);

    Task EndAsync(CancellationToken cancellationToken = default);

    Task<string?> GetCurrentProcessAsync(CancellationToken ct = default);

    Task<DateTime?> GetClosingDatetimeAsync(CancellationToken ct = default);

}
