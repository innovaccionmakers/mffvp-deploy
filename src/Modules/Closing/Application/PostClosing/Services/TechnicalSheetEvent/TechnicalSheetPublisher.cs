
using Closing.IntegrationEvents;
using Common.SharedKernel.Application.EventBus;

namespace Closing.Application.PostClosing.Services.TechnicalSheetEvent;

public class TechnicalSheetPublisher(IEventBus eventBus) : ITechnicalSheetPublisher
{
    public async Task PublishAsync(DateOnly closingDate, CancellationToken cancellationToken)
    {
        var @event = new TechnicalSheetDataBuilderEvent(closingDate);

        await eventBus.PublishAsync(@event, cancellationToken);
    }
}
