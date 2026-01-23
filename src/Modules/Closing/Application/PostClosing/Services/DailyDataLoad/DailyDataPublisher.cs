using Closing.IntegrationEvents.DailyDataLoad;
using Common.SharedKernel.Application.EventBus;


namespace Closing.Application.PostClosing.Services.DailyDataLoad;

public sealed class DailyDataPublisher(IEventBus eventBus) : IDailyDataPublisher
{
    public async Task PublishAsync(int portfolioId, DateTime closingDatetime,  CancellationToken cancellationToken)
    {
        var evt = new DailyDataIntegrationEvent(
        portfolioId,
        closingDatetime
        );
        await eventBus.PublishAsync(evt, cancellationToken);
    }
}