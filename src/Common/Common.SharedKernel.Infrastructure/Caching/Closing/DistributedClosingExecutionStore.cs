using Common.SharedKernel.Application.Caching.Closing;
using Common.SharedKernel.Application.Caching.Closing.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Common.SharedKernel.Infrastructure.Caching.Closing;

internal sealed class DistributedClosingExecutionStore(
    IDistributedCache cache,
    IClosingExecutionSerializer serializer,
    TimeSpan? expirationOverride = null) 
    : IClosingExecutionStore
{
    private const string KeyPrefix = "closingExecution";
    private readonly TimeSpan _expiration = expirationOverride ?? TimeSpan.FromHours(24);

    private string GetKey(int portfolioId) => $"{KeyPrefix}:{portfolioId}";

    public async Task<bool> IsClosingActiveAsync(int portfolioId, CancellationToken cancellationToken = default)
    {
        var key = GetKey(portfolioId);
        var value = await cache.GetStringAsync(key, cancellationToken);
        return value != null;
    }

    public async Task BeginAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken = default)
    {
        var state = new ClosingExecutionState(closingDate, DateTime.UtcNow, ClosingProcess.Begin);
        var options = GetEntryOptions();
        await cache.SetStringAsync(GetKey(portfolioId), serializer.Serialize(state), options, cancellationToken);
    }

    public async Task UpdateProcessAsync(int portfolioId, string process, CancellationToken cancellationToken = default)
    {
        var key = GetKey(portfolioId);
        var json = await cache.GetStringAsync(key, cancellationToken);
        if (json is null) return;

        var current = serializer.Deserialize(json);
        if (current is null) return;

        var updated = current with { Process = process, ProcessDatetime = DateTime.UtcNow };
        var options = GetEntryOptions();

        await cache.SetStringAsync(key, serializer.Serialize(updated), options, cancellationToken);
    }

    public async Task EndAsync(int portfolioId, CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync(GetKey(portfolioId), cancellationToken);
    }

    public async Task<string?> GetCurrentProcessAsync(int portfolioId, CancellationToken ct = default)
    {
        var key = GetKey(portfolioId);
        var json = await cache.GetStringAsync(key, ct);
        if (json is null) return null;

        var state = serializer.Deserialize(json);
        return state?.Process;
    }

    public async Task<DateTime?> GetClosingDatetimeAsync(int portfolioId, CancellationToken ct = default)
    {
        var key = GetKey(portfolioId);
        var json = await cache.GetStringAsync(key, ct);
        if (json is null) return null;

        var state = serializer.Deserialize(json);
        return state?.ClosingDatetime;
    }


    private DistributedCacheEntryOptions GetEntryOptions() =>
        new() { AbsoluteExpirationRelativeToNow = _expiration };
}