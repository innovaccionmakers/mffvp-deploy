using Common.SharedKernel.Application.EventBus;

using DotNetCore.CAP;

namespace Common.SharedKernel.Infrastructure.EventBus;

internal sealed class EventBus(ICapPublisher capPublisher) : IEventBus
{
    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
    {
        var eventName = typeof(T).Name;
        await capPublisher.PublishAsync(eventName, integrationEvent);
    }
}
