namespace Common.SharedKernel.Application.Caching.Closing.Interfaces;

public interface IClosingExecutionStore
{
    Task<bool> IsClosingActiveAsync(int portfolioId, CancellationToken cancellationToken = default);

    Task BeginAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken = default);

    Task UpdateProcessAsync(int portfolioId, string process, CancellationToken cancellationToken = default);

    Task EndAsync(int portfolioId, CancellationToken cancellationToken = default);

    Task<string?> GetCurrentProcessAsync(int portfolioId, CancellationToken ct = default);

    Task<DateTime?> GetClosingDatetimeAsync(int portfolioId, CancellationToken ct = default);

}
