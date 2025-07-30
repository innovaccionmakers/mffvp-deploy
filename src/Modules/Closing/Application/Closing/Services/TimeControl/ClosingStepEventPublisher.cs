using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.IntegrationEvents.ClosingStep;
using Common.SharedKernel.Application.EventBus;

public sealed class ClosingStepEventPublisher(IEventBus eventBus) : IClosingStepEventPublisher
{
    public async Task PublishAsync(int portfolioId, string process, DateTime closingDatetime, DateTime processDatetime, CancellationToken cancellationToken)
    {
        var evt = new ClosingStepIntegrationEvent(
            portfolioId,
            closingDatetime,
            processDatetime,
            process);

        await eventBus.PublishAsync(evt, cancellationToken);
    }
}