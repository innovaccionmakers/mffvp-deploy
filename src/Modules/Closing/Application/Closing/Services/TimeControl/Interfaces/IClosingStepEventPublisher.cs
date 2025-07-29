using Common.SharedKernel.Application.EventBus;
using Closing.IntegrationEvents.ClosingStep;
using Closing.IntegrationEvents.ProcessPendingContributionsRequested;
using Common.SharedKernel.Application.Caching.Closing;

namespace Closing.Application.Closing.Services.TimeControl.Interrfaces;

public interface IClosingStepEventPublisher
{
    Task PublishAsync(int portfolioId, string process, DateTime closingDatetime, CancellationToken cancellationToken);
}
