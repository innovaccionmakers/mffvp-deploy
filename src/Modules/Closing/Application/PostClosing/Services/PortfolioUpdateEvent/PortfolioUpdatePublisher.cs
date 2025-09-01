
using Closing.IntegrationEvents.PostClosing;
using Common.SharedKernel.Application.EventBus;

namespace Closing.Application.PostClosing.Services.PortfolioUpdateEvent;


/// <summary>
/// Publica un <see cref="PortfolioUpdatedIntegrationEvent"/> con el portafolio y la fecha de cierre,
/// notificando la actualización del portafolio  al dominio Product mediante <see cref="IEventBus"/>.
/// </summary>

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