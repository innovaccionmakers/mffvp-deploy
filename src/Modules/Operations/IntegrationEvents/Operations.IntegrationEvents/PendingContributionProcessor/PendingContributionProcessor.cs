using Closing.IntegrationEvents.ProcessPendingContributionsRequested;
using DotNetCore.CAP;
using MediatR;
using Operations.Integrations.Contributions.ProcessPendingContributions;


namespace Operations.IntegrationEvents.PendingContributionProcessor;

public sealed class PendingContributionProcessor(ISender mediator) : ICapSubscribe
{
    [CapSubscribe(nameof(ProcessPendingContributionsRequestedIntegrationEvent))]
    public async Task HandleAsync(
        ProcessPendingContributionsRequestedIntegrationEvent message,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new ProcessPendingContributionsCommand(message.PortfolioId),
            cancellationToken);
    }
}