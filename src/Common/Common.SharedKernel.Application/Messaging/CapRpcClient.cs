using System.Collections.Concurrent;
using System.Text.Json;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Common.SharedKernel.Application.Messaging;

public interface ICapRpcClient
{
    Task<TResponse> CallAsync<TRequest, TResponse>(
        string topic,
        TRequest request,
        TimeSpan timeout,
        CancellationToken ct);
}

public sealed class CapRpcClient : ICapRpcClient
{
    public static class Headers
    {
        public const string CorrelationId = "x-rpc-cid";
    }

    public static class Topics
    {
        public const string Callback = "rpc.callback";
    }

    private readonly ICapPublisher _cap;
    private readonly ILogger<CapRpcClient> _log;
    private readonly JsonSerializerOptions _jsonOptions;

    private static readonly ConcurrentDictionary<string, TaskCompletionSource<JsonElement>> _pending =
        new(StringComparer.OrdinalIgnoreCase);

    public CapRpcClient(ICapPublisher cap, IOptions<JsonSerializerOptions> jsonOptions, ILogger<CapRpcClient> log)
    {
        _cap = cap;
        _log = log;
        _jsonOptions = jsonOptions.Value;
    }

    public async Task<TResponse> CallAsync<TRequest, TResponse>(string requestTopic, TRequest request, TimeSpan timeout,
        CancellationToken ct)
    {
        var cid = Guid.NewGuid().ToString("N");
        var tcs = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!_pending.TryAdd(cid, tcs)) throw new InvalidOperationException($"Duplicated cid {cid}");

        var headers = new Dictionary<string, string?>
        {
            [DotNetCore.CAP.Messages.Headers.CallbackName] = Topics.Callback,
            [Headers.CorrelationId] = cid
        };

        await _cap.PublishAsync(requestTopic, request!, headers, ct);

        using var reg = ct.Register(() => tcs.TrySetCanceled(ct));
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(timeout);

        try
        {
            var completed = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, timeoutCts.Token));
            if (completed != tcs.Task)
            {
                if (ct.IsCancellationRequested) ct.ThrowIfCancellationRequested();
                throw new TimeoutException($"RPC {requestTopic} timeout {timeout.TotalSeconds}s");
            }

            var json = await tcs.Task;
            return JsonSerializer.Deserialize<TResponse>(json.GetRawText(), _jsonOptions)!;
        }
        finally
        {
            _pending.TryRemove(cid, out _);
        }
    }

    public static void Complete(string cid, JsonElement data)
    {
        if (_pending.TryRemove(cid, out var tcs)) tcs.TrySetResult(data);
    }
}