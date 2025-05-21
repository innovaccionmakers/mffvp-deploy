using System.Text.Json;
using Common.SharedKernel.Application.Messaging;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;

namespace Common.SharedKernel.Infrastructure.EventBus;

public class CapCallbackSubscriber : ICapSubscribe
{
    private readonly ILogger<CapCallbackSubscriber> _log;

    public CapCallbackSubscriber(ILogger<CapCallbackSubscriber> log)
    {
        _log = log;
    }

    [CapSubscribe(CapRpcClient.Topics.Callback, Group = CapRpcClient.Topics.Callback)]
    public void OnCallback(JsonElement json, [FromCap] CapHeader header)
    {
        if (!header.TryGetValue(CapRpcClient.Headers.CorrelationId, out var cid))
        {
            _log.LogWarning("Callback without cid");
            return;
        }

        _log.LogDebug("Callback cid={Cid}", cid);
        CapRpcClient.Complete(cid!, json);
    }
}