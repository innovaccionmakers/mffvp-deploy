
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.IntegrationEvents.PostClosing.ProcessPendingContributionsRequested;
using Common.SharedKernel.Application.EventBus;

namespace Closing.Application.PostClosing.Services.PendingTransactionHandler;

public class PendingTransactionHandler : IPendingTransactionHandler
{
    private readonly IEventBus _eventBus;
    private readonly ITimeControlService _timeControlService;

    public PendingTransactionHandler(
        IEventBus eventBus,
        ITimeControlService timeControlService)
    {
        _eventBus = eventBus;
        _timeControlService = timeControlService;
    }

    public async Task HandleAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        // 1. Publicar evento para procesar aportes pendientes
        var pendingEvent = new ProcessPendingContributionsRequestedIntegrationEvent(portfolioId);
        await _eventBus.PublishAsync(pendingEvent, cancellationToken);

        // 2. Una vez procesados los aportes pendientes, finalizar el flujo de cierre
        await _timeControlService.EndAsync(portfolioId, cancellationToken);
    }
}