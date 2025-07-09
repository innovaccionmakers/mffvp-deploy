using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Common.SharedKernel.Application.Closing;

namespace Common.SharedKernel.Infrastructure.Caching;

internal sealed class RedisClosingExecutionStore(IDistributedCache cache) : IClosingExecutionStore
{
    private const string KeyPrefix = "closingExecution";

    public async Task<bool> IsClosingActiveAsync(int portfolioId, CancellationToken cancellationToken = default)
    {
        var key = $"{KeyPrefix}:{portfolioId}";
        var value = await cache.GetStringAsync(key, cancellationToken);
        return value != null;
    }

    public async Task BeginAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken = default)
    {
        var state = new ClosingExecutionState(closingDate, DateTime.UtcNow, ClosingProcess.Begin);
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) };
        await cache.SetStringAsync($"{KeyPrefix}:{portfolioId}", JsonSerializer.Serialize(state), options, cancellationToken);
    }

    public async Task UpdateProcessAsync(int portfolioId, ClosingProcess process, CancellationToken cancellationToken = default)
    {
        var key = $"{KeyPrefix}:{portfolioId}";
        var json = await cache.GetStringAsync(key, cancellationToken);
        if (json is null) return;
        var current = JsonSerializer.Deserialize<ClosingExecutionState>(json)!;
        var state = current with { Process = process, ProcessDatetime = DateTime.UtcNow };
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) };
        await cache.SetStringAsync(key, JsonSerializer.Serialize(state), options, cancellationToken);
    }

    public async Task EndAsync(int portfolioId, CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync($"{KeyPrefix}:{portfolioId}", cancellationToken);
    }
}
