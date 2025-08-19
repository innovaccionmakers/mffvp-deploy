using Closing.Domain.PortfolioValuations;
using Closing.IntegrationEvents.PostClosing;
using Common.SharedKernel.Application.EventBus;

namespace Closing.Application.PostClosing.Services.PortfolioUpdateEvent;
public sealed class PortfolioUpdatePublisher(
    IPortfolioValuationRepository repository,
    IEventBus eventBus)
    : IPortfolioUpdatePublisher
{
    public async Task PublishAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        var valuation = await repository.GetValuationAsync(portfolioId, closingDate, cancellationToken);

        if (valuation is null)
        {
            return; // TODO: Validar si es necesario lanzar una excepción o registrar un error.
        }

        var @event = new PortfolioUpdatedIntegrationEvent(
            valuation.PortfolioId,
            valuation.ClosingDate
        );

        await eventBus.PublishAsync(@event, cancellationToken);
    }
}