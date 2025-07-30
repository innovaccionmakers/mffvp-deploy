using Closing.Domain.PortfolioValuations;
using Closing.IntegrationEvents.PostClosing;
using Common.SharedKernel.Application.EventBus;

namespace Closing.Application.PostClosing.Services.PortfolioValuationEvent;
public sealed class PortfolioValuationPublisher(
    IPortfolioValuationRepository repository,
    IEventBus eventBus)
    : IPortfolioValuationPublisher
{
    public async Task PublishAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        var valuation = await repository.GetValuationAsync(portfolioId, closingDate, cancellationToken);

        if (valuation is null)
        {
            return; // TODO: Validar si es necesario lanzar una excepción o registrar un error.
        }

        var @event = new PortfolioValuationUpdatedIntegrationEvent(
            valuation.PortfolioId,
            valuation.ClosingDate,
            valuation.Amount,
            valuation.Units,
            valuation.UnitValue,
            valuation.GrossYieldPerUnit,
            valuation.CostPerUnit,
            valuation.DailyProfitability,
            valuation.IncomingOperations,
            valuation.OutgoingOperations,
            valuation.ProcessDate
        );

        await eventBus.PublishAsync(@event, cancellationToken);
    }
}