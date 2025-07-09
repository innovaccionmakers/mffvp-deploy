namespace Common.SharedKernel.Application.Closing;

public interface IClosingExecutionStore
{
    Task<bool> IsClosingActiveAsync(int portfolioId, CancellationToken cancellationToken = default);

    Task BeginAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken = default);

    Task UpdateProcessAsync(int portfolioId, ClosingProcess process, CancellationToken cancellationToken = default);

    Task EndAsync(int portfolioId, CancellationToken cancellationToken = default);
}
