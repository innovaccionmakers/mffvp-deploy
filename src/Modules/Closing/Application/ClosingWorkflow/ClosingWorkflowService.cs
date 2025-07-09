using Common.SharedKernel.Application.EventBus;
using Common.SharedKernel.Application.Closing;
using Closing.IntegrationEvents.ClosingStep;
using Closing.IntegrationEvents.ProcessPendingContributionsRequested;

namespace Closing.Application.ClosingWorkflow;

public sealed class ClosingWorkflowService(
    IClosingExecutionStore store,
    IEventBus eventBus) : IClosingWorkflowService
{
    public async Task BeginAsync(int portfolioId, DateTime closingDate, CancellationToken ct = default)
    {
        await store.BeginAsync(portfolioId, closingDate, ct);
        var evt = new ClosingStepIntegrationEvent(portfolioId, closingDate, DateTime.UtcNow, ClosingProcess.Begin.ToString());
        await eventBus.PublishAsync(evt, ct);
    }

    public async Task AdvanceAsync(int portfolioId, ClosingProcess process, CancellationToken ct = default)
    {
        await store.UpdateProcessAsync(portfolioId, process, ct);
        var evt = new ClosingStepIntegrationEvent(portfolioId, DateTime.UtcNow, DateTime.UtcNow, process.ToString());
        await eventBus.PublishAsync(evt, ct);
    }

    public async Task EndAsync(int portfolioId, CancellationToken ct = default)
    {
        await store.EndAsync(portfolioId, ct);
        var evt = new ProcessPendingContributionsRequestedIntegrationEvent(portfolioId);
        await eventBus.PublishAsync(evt, ct);
    }
}
