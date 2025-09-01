
using Closing.Domain.TrustYields;
using Closing.IntegrationEvents.PostClosing;
using Common.SharedKernel.Application.EventBus;

namespace Closing.Application.PostClosing.Services.TrustYieldEvent;

/// <summary>
/// Publica un <see cref="TrustYieldGeneratedIntegrationEvent"/> por cada rendimiento de fideicomiso
/// leído desde <see cref="ITrustYieldRepository"/>, notificando saldos, rendimientos y retenciones
/// al dominio Operations mediante <see cref="IEventBus"/>.
/// </summary>

public sealed class TrustYieldPublisher(
    ITrustYieldRepository repository,
    IEventBus eventBus)
    : ITrustYieldPublisher
{
    public async Task PublishAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        var trustYields = await repository.GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDate, cancellationToken);

        foreach (var trustYield in trustYields)
        {
            //var shouldEmitEvent =
            //    trustYield.YieldAmount != 0 ||
            //    trustYield.PreClosingBalance != trustYield.ClosingBalance ||
            //    trustYield.PreClosingBalance == trustYield.Capital;

            //if (!shouldEmitEvent)
            //    continue;

            var @event = new TrustYieldGeneratedIntegrationEvent(
                trustYield.TrustId,
                trustYield.PortfolioId,
                trustYield.ClosingDate,
                trustYield.YieldAmount,
                trustYield.ClosingBalance,
                trustYield.YieldRetention,
                trustYield.Units,
                trustYield.ProcessDate
            );

            await eventBus.PublishAsync(@event, cancellationToken);
        }
    }
}