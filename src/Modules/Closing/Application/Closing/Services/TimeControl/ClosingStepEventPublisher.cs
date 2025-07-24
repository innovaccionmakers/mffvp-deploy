//using Common.SharedKernel.Application.EventBus;
//using Closing.IntegrationEvents.ClosingStep;
//using Closing.IntegrationEvents.ProcessPendingContributionsRequested;
//using Common.SharedKernel.Application.Caching.Closing;
//using Common.SharedKernel.Application.Caching.Closing.Interfaces;
//using Common.SharedKernel.Application.EventBus;

//namespace Closing.Application.Closing.Services.TimeControl;

//public sealed class ClosingStepEventPublisher(
//    IClosingExecutionStore store,
//    IEventBus eventBus) : IClosingStepEventPublisher
//{
//    public async Task BeginAsync(int portfolioId, DateTime closingDate, CancellationToken ct = default)
//    {
//        await store.BeginAsync(portfolioId, closingDate, ct);
//        var evt = new ClosingStepIntegrationEvent(portfolioId, closingDate, DateTime.UtcNow, ClosingProcess.Begin.ToString());
//        await eventBus.PublishAsync(evt, ct);
//    }

//    public async Task AdvanceAsync(int portfolioId, ClosingProcess process, CancellationToken ct = default)
//    {
//        await store.UpdateProcessAsync(portfolioId, process.ToString(), ct);
//        var evt = new ClosingStepIntegrationEvent(portfolioId, DateTime.UtcNow, DateTime.UtcNow, process.ToString());
//        await eventBus.PublishAsync(evt, ct);
//    }

//    public async Task EndAsync(int portfolioId, CancellationToken ct = default)
//    {
//        await store.EndAsync(portfolioId, ct);
//        var evt = new ProcessPendingContributionsRequestedIntegrationEvent(portfolioId);
//ProcessPendingContributionsRequestedIntegrationEvent no pertenece a ClosingStepEventPublisher, sino al proceso que ocurre después de que se completa el cierre.
//Pendiente publicarlo desde otro componente, el consumer CAP cuando reciba el "ClosingEnd".
//        await eventBus.PublishAsync(evt, ct);
//    }
//}

using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.IntegrationEvents.ClosingStep;
using Common.SharedKernel.Application.EventBus;

public sealed class ClosingStepEventPublisher(IEventBus eventBus) : IClosingStepEventPublisher
{
    public async Task PublishAsync(int portfolioId, string process, DateTime closingDatetime, CancellationToken cancellationToken)
    {
        var evt = new ClosingStepIntegrationEvent(
            portfolioId,
            closingDatetime,
            DateTime.UtcNow,
            process);

        await eventBus.PublishAsync(evt, cancellationToken);
    }
}