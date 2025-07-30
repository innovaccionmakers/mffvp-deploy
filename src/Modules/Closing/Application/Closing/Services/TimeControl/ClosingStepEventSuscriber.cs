
using Closing.IntegrationEvents.ClosingStep;
using Common.SharedKernel.Application.Caching.Closing;
using Common.SharedKernel.Application.Caching.Closing.Interfaces;
using DotNetCore.CAP;

namespace Closing.Application.Closing.Services.TimeControl;

public sealed class ClosingStepEventSuscriber(IClosingExecutionStore store) : ICapSubscribe
{
    [CapSubscribe(nameof(ClosingStepIntegrationEvent))]
    public async Task HandleAsync(ClosingStepIntegrationEvent evt, CancellationToken cancellationToken)
    {
        if (evt.Process == ClosingProcess.End.ToString())
        {
            await store.EndAsync(evt.PortfolioId, cancellationToken);
            return;
        }

        await store.UpdateProcessAsync(evt.PortfolioId, evt.Process, evt.ProcessDatetime, cancellationToken);
    }
}
