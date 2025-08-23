
using Closing.IntegrationEvents.PostClosing;
using Common.SharedKernel.Application.EventBus;

namespace Closing.Application.PostClosing.Services.PortfolioUpdateEvent;
public sealed class PortfolioUpdatePublisher(
    IEventBus eventBus)
    : IPortfolioUpdatePublisher
{
    public async Task PublishAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {

        var @event = new PortfolioUpdatedIntegrationEvent(
            portfolioId,
            closingDate
        );

        await eventBus.PublishAsync(@event, cancellationToken);
    }
}