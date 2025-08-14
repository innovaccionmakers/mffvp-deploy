using Common.SharedKernel.Application.Caching.Closing;
using Common.SharedKernel.Application.Caching.Closing.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Common.SharedKernel.Infrastructure.Caching.Closing;

internal sealed class DistributedClosingExecutionStore(
    IDistributedCache cache,
    IClosingExecutionSerializer serializer) 
    : IClosingExecutionStore
{
    private const string KeyPrefix = "closingExecution";

    private string GetKeyByPortfolio(int portfolioId) => $"{KeyPrefix}:{portfolioId}";

    private string GetKey => $"{KeyPrefix}";

    public async Task<bool> IsClosingActiveAsync(CancellationToken cancellationToken = default)
    {
        var key = GetKey;
        var value = await cache.GetStringAsync(key, cancellationToken);
        return value != null;
    }

    public async Task BeginAsync(DateTime closingBeginTime, CancellationToken cancellationToken = default)
    {
        var state = new ClosingExecutionState(closingBeginTime, closingBeginTime, ClosingProcess.Begin);
        await cache.SetStringAsync(GetKey, serializer.Serialize(state), cancellationToken);
    }

    public async Task UpdateProcessAsync(string process, DateTime processDatetime, CancellationToken cancellationToken = default)
    {
        var key = GetKey;
        var json = await cache.GetStringAsync(key, cancellationToken);
        if (json is null) return;

        var current = serializer.Deserialize(json);
        if (current is null) return;

        var updated = current with { Process = process, ProcessDatetime = processDatetime };;

        await cache.SetStringAsync(key, serializer.Serialize(updated), cancellationToken);
    }

    public async Task EndAsync(CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync(GetKey, cancellationToken);
    }

    public async Task<string?> GetCurrentProcessAsync(CancellationToken ct = default)
    {
        var key = GetKey;
        var json = await cache.GetStringAsync(key, ct);
        if (json is null) return null;

        var state = serializer.Deserialize(json);
        return state?.Process;
    }

    public async Task<DateTime?> GetClosingDatetimeAsync(CancellationToken ct = default)
    {
        var key = GetKey;
        var json = await cache.GetStringAsync(key, ct);
        if (json is null) return null;

        var state = serializer.Deserialize(json);
        return state?.ClosingDatetime;
    }
}