using Common.SharedKernel.Application.EventBus;
using Closing.IntegrationEvents.ClosingStep;
using Closing.IntegrationEvents.ProcessPendingContributionsRequested;
using Common.SharedKernel.Application.Caching.Closing;

namespace Closing.Application.ClosingWorkflow;

public interface IClosingWorkflowService
{
    Task BeginAsync(int portfolioId, DateTime closingDate, CancellationToken ct = default);
    Task AdvanceAsync(int portfolioId, ClosingProcess process, CancellationToken ct = default);
    Task EndAsync(int portfolioId, CancellationToken ct = default);
}
