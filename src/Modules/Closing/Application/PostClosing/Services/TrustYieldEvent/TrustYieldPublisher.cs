
using Closing.Domain.TrustYields;
using Closing.IntegrationEvents.PostClosing;
using Common.SharedKernel.Application.EventBus;

namespace Closing.Application.PostClosing.Services.TrustYieldEvent;

public sealed class TrustYieldPublisher(
    ITrustYieldRepository repository,
    IEventBus eventBus)
    : ITrustYieldPublisher
{
    public async Task PublishAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        var trustYields = await repository.GetByPortfolioAndDateAsync(portfolioId, closingDate, cancellationToken);

        foreach (var trustYield in trustYields)
        {
            var shouldEmitEvent =
                trustYield.YieldAmount != 0 ||
                trustYield.PreClosingBalance != trustYield.ClosingBalance;

            if (!shouldEmitEvent)
                continue;

            var @event = new TrustYieldGeneratedIntegrationEvent(
                trustYield.TrustId,
                trustYield.PortfolioId,
                trustYield.ClosingDate,
                trustYield.YieldAmount,
                trustYield.ClosingBalance,
                trustYield.YieldRetention,
                trustYield.ProcessDate
            );

            await eventBus.PublishAsync(@event, cancellationToken);
        }
    }
}